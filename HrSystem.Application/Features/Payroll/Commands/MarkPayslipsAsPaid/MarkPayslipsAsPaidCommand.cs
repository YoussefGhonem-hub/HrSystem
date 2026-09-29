using ErrorOr;
using HrSystem.Application.Features.Payroll.Common;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Payroll.Commands.MarkPayslipsAsPaid;

public record MarkPayslipsAsPaidCommand(
    int Month,
    int Year,
    List<Guid>? PayslipIds = null
) : IRequest<ErrorOr<GenericResponse<MarkPayslipsAsPaidResultDto>>>;

public class MarkPayslipsAsPaidResultDto
{
    public int TotalMarked { get; set; }
    public int AlreadyPaid { get; set; }
    public decimal TotalAmountPaid { get; set; }
    public string CycleName { get; set; } = string.Empty;
}

public class MarkPayslipsAsPaidCommandHandler
    : IRequestHandler<MarkPayslipsAsPaidCommand, ErrorOr<GenericResponse<MarkPayslipsAsPaidResultDto>>>
{
    private readonly ApplicationDbContext _context;

    public MarkPayslipsAsPaidCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<MarkPayslipsAsPaidResultDto>>> Handle(
        MarkPayslipsAsPaidCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Month < 1 || request.Month > 12)
            return Error.Validation(description: "Month must be between 1 and 12.");
        if (request.Year < 2000 || request.Year > 2100)
            return Error.Validation(description: "Invalid year.");

        var isSuperOrOrgAdmin = CurrentUser.Roles?.Contains(RoleNames.SuperAdmin) == true
            || CurrentUser.Roles?.Contains(RoleNames.OrganizationAdmin) == true;
        var branchId = isSuperOrOrgAdmin ? (Guid?)null : CurrentUser.BranchId;

        // A month may hold several cycles (weekly / bi-weekly pay periods); mark them all.
        var cycles = await _context.PayrollCycles
            .Where(c => c.Month == request.Month && c.Year == request.Year)
            .OrderBy(c => c.PeriodStartDate)
            .ToListAsync(cancellationToken);

        if (cycles.Count == 0)
            return Error.NotFound(description: $"No payroll cycle found for {request.Month}/{request.Year}.");

        var cycleIds = cycles.Select(c => c.Id).ToList();

        var query = _context.Payslips
            .Include(p => p.Employee)
            .Include(p => p.PayslipDeductions)
            .Where(p => !p.IsDeleted && cycleIds.Contains(p.PayrollCycleId));

        if (branchId.HasValue)
            query = query.Where(p => p.Employee.BranchId == branchId.Value);

        // If specific payslip IDs are provided, filter by them
        if (request.PayslipIds != null && request.PayslipIds.Count > 0)
            query = query.Where(p => request.PayslipIds.Contains(p.Id));

        var payslips = await query.ToListAsync(cancellationToken);

        var result = new MarkPayslipsAsPaidResultDto
        {
            CycleName = string.Join(", ", cycles.Select(c => c.CycleName))
        };

        var payslipsToMark = payslips.Where(p => !p.IsPaid).ToList();
        var loanIds = payslipsToMark
            .SelectMany(p => p.PayslipDeductions)
            .Where(d => !d.IsDeleted && d.LoanId.HasValue)
            .Select(d => d.LoanId!.Value)
            .Distinct()
            .ToList();

        var loansById = loanIds.Count == 0
            ? new Dictionary<Guid, Domain.Entities.Payroll.Loan>()
            : await _context.Loans
                .Where(l => loanIds.Contains(l.Id))
                .ToDictionaryAsync(l => l.Id, cancellationToken);

        var now = DateTime.UtcNow;
        foreach (var payslip in payslips)
        {
            if (payslip.IsPaid)
            {
                result.AlreadyPaid++;
                continue;
            }

            payslip.IsPaid = true;
            payslip.PaidDate = now;

            foreach (var deduction in payslip.PayslipDeductions.Where(d => !d.IsDeleted && d.LoanId.HasValue))
            {
                if (!loansById.TryGetValue(deduction.LoanId!.Value, out var loan))
                    continue;

                if (loan.IsDeleted || loan.RemainingAmount <= 0)
                    continue;

                var paidInstallmentAmount = Math.Min(deduction.Amount, loan.RemainingAmount);
                if (paidInstallmentAmount <= 0)
                    continue;

                loan.RemainingAmount -= paidInstallmentAmount;
                if (loan.RemainingAmount <= 0)
                {
                    loan.RemainingAmount = 0;
                    loan.IsActive = false;
                    loan.EndDate = payslip.PaidDate ?? now;
                }
            }

            result.TotalMarked++;
            result.TotalAmountPaid += payslip.NetSalary;
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Update each cycle's status: Paid when every payslip is paid, otherwise Processed.
        foreach (var cycle in cycles)
        {
            var allPaid = !await _context.Payslips
                .AnyAsync(p => !p.IsDeleted && p.PayrollCycleId == cycle.Id && !p.IsPaid, cancellationToken);

            if (allPaid)
            {
                cycle.StatusId = PayrollStatusIds.Paid;
                cycle.PaymentDate = now;
            }
            else
            {
                cycle.StatusId = PayrollStatusIds.Processed;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return GenericResponse<MarkPayslipsAsPaidResultDto>.SuccessResult(
            result,
            $"{result.TotalMarked} payslips marked as paid. {result.AlreadyPaid} were already paid.");
    }
}
