using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;
using HrSystem.Application.Features.Payroll.Queries.GetMyPayslipDetails;

namespace HrSystem.Application.Features.Payroll.Queries.GetPayslipDetails;

/// <summary>
/// Query to get full payslip details for HR/Admin - can view any employee's payslip
/// </summary>
public record GetPayslipDetailsQuery(Guid PayslipId) : IRequest<ErrorOr<GenericResponse<PayslipDetailsDto>>>;

public class GetPayslipDetailsQueryHandler : IRequestHandler<GetPayslipDetailsQuery, ErrorOr<GenericResponse<PayslipDetailsDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetPayslipDetailsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<PayslipDetailsDto>>> Handle(
        GetPayslipDetailsQuery request,
        CancellationToken cancellationToken)
    {
        var roles = CurrentUser.Roles;
        var isHrOrAdmin = roles.Any(r =>
            string.Equals(r, RoleNames.SuperAdmin, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(r, RoleNames.OrganizationAdmin, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(r, RoleNames.HRManager, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(r, RoleNames.HRSpecialist, StringComparison.OrdinalIgnoreCase));

        if (!isHrOrAdmin)
        {
            return Error.Forbidden("Payslip.Forbidden", "You do not have permission to view this payslip");
        }

        // Apply branch scope for HR managers (not super/org admins)
        var isSuperOrOrgAdmin = roles.Any(r =>
            string.Equals(r, RoleNames.SuperAdmin, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(r, RoleNames.OrganizationAdmin, StringComparison.OrdinalIgnoreCase));
        var branchId = isSuperOrOrgAdmin ? (Guid?)null : CurrentUser.BranchId;

        var query = _context.Payslips
            .Include(p => p.PayrollCycle)
            .Include(p => p.Employee)
            .Include(p => p.PayslipAllowances)
            .Include(p => p.PayslipDeductions)
            .Where(p => !p.IsDeleted && p.Id == request.PayslipId);

        if (branchId.HasValue)
        {
            query = query.Where(p => p.Employee.BranchId == branchId.Value);
        }

        var payslip = await query
            .Select(p => new PayslipDetailsDto
            {
                PayslipId = p.Id,
                PayrollCycleId = p.PayrollCycleId,
                Year = p.PayrollCycle.Year,
                Month = p.PayrollCycle.Month,
                PayslipNumber = p.PayslipNumber,
                EmployeeId = p.EmployeeId,
                EmployeeCode = p.Employee.EmployeeCode,
                EmployeeNameEn = p.Employee.FirstNameEn + " " + p.Employee.LastNameEn,
                EmployeeNameAr = p.Employee.FirstNameAr + " " + p.Employee.LastNameAr,
                DepartmentName = p.Employee.Department != null ? p.Employee.Department.NameEn : "",
                BasicSalary = p.BasicSalary,
                TotalAllowances = p.TotalAllowances,
                GrossSalary = p.GrossSalary,
                TotalDeductions = p.TotalDeductions,
                IncomeTax = p.IncomeTax,
                SocialInsuranceEmployee = p.SocialInsuranceEmployee,
                SocialInsuranceEmployer = p.SocialInsuranceEmployer,
                OvertimeAmount = p.OvertimeAmount,
                BonusAmount = p.BonusAmount,
                LeaveDeductions = p.LeaveDeductions,
                UnpaidLeaveDays = p.UnpaidLeaveDays,
                NetSalary = p.NetSalary,
                TotalWorkingDays = p.TotalWorkingDays,
                ActualWorkingDays = p.ActualWorkingDays,
                AbsentDays = p.AbsentDays,
                PdfFileUrl = p.PdfFileUrl,
                GeneratedDate = p.GeneratedDate,
                IsPaid = p.IsPaid,
                PaidDate = p.PaidDate,
                Allowances = p.PayslipAllowances.Select(a => new MyPayslipAllowanceDto
                {
                    AllowanceNameAr = a.AllowanceNameAr,
                    AllowanceNameEn = a.AllowanceNameEn,
                    Amount = a.Amount
                }).ToList(),
                Deductions = p.PayslipDeductions.Select(d => new MyPayslipDeductionDto
                {
                    DeductionNameAr = d.DeductionNameAr,
                    DeductionNameEn = d.DeductionNameEn,
                    Amount = d.Amount
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (payslip is null)
        {
            return Error.NotFound("Payslip.NotFound", "Payslip not found");
        }

        return new GenericResponse<PayslipDetailsDto>
        {
            Success = true,
            Message = "Payslip details retrieved successfully",
            Data = payslip
        };
    }
}
