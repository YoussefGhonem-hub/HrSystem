using ErrorOr;
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

        var cycle = await _context.PayrollCycles
            .FirstOrDefaultAsync(c => c.Month == request.Month && c.Year == request.Year, cancellationToken);

        if (cycle == null)
            return Error.NotFound(description: $"No payroll cycle found for {request.Month}/{request.Year}.");

        var query = _context.Payslips
            .Include(p => p.Employee)
            .Where(p => !p.IsDeleted && p.PayrollCycleId == cycle.Id);

        if (branchId.HasValue)
            query = query.Where(p => p.Employee.BranchId == branchId.Value);

        // If specific payslip IDs are provided, filter by them
        if (request.PayslipIds != null && request.PayslipIds.Count > 0)
            query = query.Where(p => request.PayslipIds.Contains(p.Id));

        var payslips = await query.ToListAsync(cancellationToken);

        var result = new MarkPayslipsAsPaidResultDto
        {
            CycleName = cycle.CycleName
        };

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
            result.TotalMarked++;
            result.TotalAmountPaid += payslip.NetSalary;
        }

        // Update cycle status to Paid if all payslips in cycle are now paid
        var allCyclePayslips = await _context.Payslips
            .Where(p => !p.IsDeleted && p.PayrollCycleId == cycle.Id)
            .ToListAsync(cancellationToken);

        if (allCyclePayslips.All(p => p.IsPaid))
        {
            cycle.StatusId = PayrollStatusIds.Paid;
            cycle.PaymentDate = now;
        }
        else
        {
            cycle.StatusId = PayrollStatusIds.Processed;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return GenericResponse<MarkPayslipsAsPaidResultDto>.SuccessResult(
            result,
            $"{result.TotalMarked} payslips marked as paid. {result.AlreadyPaid} were already paid.");
    }
}
