using ErrorOr;
using HrSystem.Application.Features.Payroll.Common;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Payroll.Commands.DeletePayslips;

public record DeletePayslipsCommand(
    int Month,
    int Year,
    Guid? EmployeeId = null
) : IRequest<ErrorOr<GenericResponse<DeletePayslipsResultDto>>>;

public class DeletePayslipsResultDto
{
    public int PayslipsDeleted { get; set; }
    public List<string> Messages { get; set; } = new();
}

public class DeletePayslipsCommandHandler
    : IRequestHandler<DeletePayslipsCommand, ErrorOr<GenericResponse<DeletePayslipsResultDto>>>
{
    private readonly ApplicationDbContext _context;

    public DeletePayslipsCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<DeletePayslipsResultDto>>> Handle(
        DeletePayslipsCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Month < 1 || request.Month > 12)
            return Error.Validation(description: "Month must be between 1 and 12.");
        if (request.Year < 2000 || request.Year > 2100)
            return Error.Validation(description: "Invalid year.");

        // Check user role - only HR managers and admins can delete payslips
        var isHRManagerOrAdmin = CurrentUser.Roles?.Contains(RoleNames.HRManager) == true
            || CurrentUser.Roles?.Contains(RoleNames.OrganizationAdmin) == true
            || CurrentUser.Roles?.Contains(RoleNames.SuperAdmin) == true;

        if (!isHRManagerOrAdmin)
            return Error.Forbidden(description: "Only HR Managers and Admins can delete payslips.");

        // A month may hold several cycles (weekly / bi-weekly pay periods)
        var cycles = await _context.PayrollCycles
            .Where(c => c.Month == request.Month && c.Year == request.Year)
            .OrderBy(c => c.PeriodStartDate)
            .ToListAsync(cancellationToken);

        if (cycles.Count == 0)
            return Error.NotFound(description: "Payroll cycle not found for the specified month and year.");

        var cycleIds = cycles.Select(c => c.Id).ToList();

        // Build query for payslips to delete (paid payslips are never deleted)
        var payslipsQuery = _context.Payslips
            .Include(p => p.PayslipAllowances)
            .Include(p => p.PayslipDeductions)
            .Where(p => cycleIds.Contains(p.PayrollCycleId) && !p.IsDeleted);

        if (request.EmployeeId.HasValue)
        {
            payslipsQuery = payslipsQuery.Where(p => p.EmployeeId == request.EmployeeId.Value);
        }

        // Apply branch scope for branch-level HR managers
        var isSuperOrOrgAdmin = CurrentUser.Roles?.Contains(RoleNames.SuperAdmin) == true
            || CurrentUser.Roles?.Contains(RoleNames.OrganizationAdmin) == true;

        if (!isSuperOrOrgAdmin && CurrentUser.BranchId.HasValue)
        {
            payslipsQuery = payslipsQuery.Where(p => p.BranchId == CurrentUser.BranchId.Value);
        }

        var payslips = await payslipsQuery.ToListAsync(cancellationToken);

        if (payslips.Count == 0)
            return Error.NotFound(description: "No payslips found to delete.");

        var result = new DeletePayslipsResultDto();
        var currentUserId = CurrentUser.Id ?? Guid.Empty;

        foreach (var payslip in payslips)
        {
            if (payslip.IsPaid)
            {
                result.Messages.Add($"Skipped payslip {payslip.PayslipNumber}: it is already paid.");
                continue;
            }

            payslip.MarkAsDeleted(currentUserId);

            foreach (var allowance in payslip.PayslipAllowances)
            {
                allowance.IsDeleted = true;
            }

            foreach (var deduction in payslip.PayslipDeductions)
            {
                deduction.IsDeleted = true;
            }

            result.PayslipsDeleted++;
            result.Messages.Add($"Deleted payslip {payslip.PayslipNumber}");
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Recompute cycle totals/status from the remaining payslips
        foreach (var cycle in cycles)
        {
            await PayrollCycleTotals.RecalculateAsync(_context, cycle, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return GenericResponse<DeletePayslipsResultDto>.SuccessResult(
            result,
            $"{result.PayslipsDeleted} payslip(s) deleted successfully.");
    }
}
