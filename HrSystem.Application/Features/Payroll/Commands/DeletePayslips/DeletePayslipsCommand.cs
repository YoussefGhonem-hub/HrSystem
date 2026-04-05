using ErrorOr;
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

        // Get payroll cycle
        var cycle = await _context.PayrollCycles
            .FirstOrDefaultAsync(c => c.Month == request.Month && c.Year == request.Year, cancellationToken);

        if (cycle == null)
            return Error.NotFound(description: "Payroll cycle not found for the specified month and year.");

        // Build query for payslips to delete
        var payslipsQuery = _context.Payslips
            .Include(p => p.PayslipAllowances)
            .Include(p => p.PayslipDeductions)
            .Where(p => p.PayrollCycleId == cycle.Id && !p.IsDeleted);

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

        var result = new DeletePayslipsResultDto
        {
            PayslipsDeleted = payslips.Count
        };

        // Soft delete payslips using the MarkAsDeleted helper
        var currentUserId = CurrentUser.Id ?? Guid.Empty;
        
        foreach (var payslip in payslips)
        {
            payslip.MarkAsDeleted(currentUserId);

            // Mark related entities as deleted
            foreach (var allowance in payslip.PayslipAllowances)
            {
                allowance.IsDeleted = true;
            }

            foreach (var deduction in payslip.PayslipDeductions)
            {
                deduction.IsDeleted = true;
            }

            result.Messages.Add($"Deleted payslip {payslip.PayslipNumber}");
        }

        // Update cycle totals
        var deletedGrossSalary = payslips.Sum(p => p.GrossSalary);
        var deletedNetSalary = payslips.Sum(p => p.NetSalary);
        var deletedDeductions = payslips.Sum(p => p.TotalDeductions);

        cycle.TotalGrossSalary -= deletedGrossSalary;
        cycle.TotalNetSalary -= deletedNetSalary;
        cycle.TotalDeductions -= deletedDeductions;
        cycle.TotalTax -= deletedDeductions;

        await _context.SaveChangesAsync(cancellationToken);

        return GenericResponse<DeletePayslipsResultDto>.SuccessResult(
            result,
            $"{result.PayslipsDeleted} payslip(s) deleted successfully.");
    }
}
