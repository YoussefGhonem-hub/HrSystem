using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Payroll.Queries.GetMyPayslipDetails;

/// <summary>
/// Query to get payslip full details for the currently logged-in user
/// </summary>
public record GetMyPayslipDetailsQuery(Guid PayslipId) : IRequest<ErrorOr<GenericResponse<MyPayslipDetailsDto>>>;

public class GetMyPayslipDetailsQueryHandler : IRequestHandler<GetMyPayslipDetailsQuery, ErrorOr<GenericResponse<MyPayslipDetailsDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetMyPayslipDetailsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<MyPayslipDetailsDto>>> Handle(
        GetMyPayslipDetailsQuery request,
        CancellationToken cancellationToken)
    {
        Guid? employeeId = CurrentUser.EmployeeId;

        if (!employeeId.HasValue || employeeId.Value == Guid.Empty)
        {
            var userId = CurrentUser.Id;
            if (userId.HasValue)
            {
                employeeId = await _context.Employees
                    .Where(e => e.UserId == userId)
                    .Select(e => e.Id)
                    .FirstOrDefaultAsync(cancellationToken);
            }
        }

        if (!employeeId.HasValue || employeeId.Value == Guid.Empty)
        {
            return Error.Unauthorized("Payslip.Unauthorized", "Current user is not linked to an employee");
        }

        var payslip = await _context.Payslips
            .Include(p => p.PayrollCycle)
            .Include(p => p.PayslipAllowances)
            .Include(p => p.PayslipDeductions)
            .Where(p => !p.IsDeleted && p.EmployeeId == employeeId.Value && p.Id == request.PayslipId)
            .Select(p => new MyPayslipDetailsDto
            {
                PayslipId = p.Id,
                PayrollCycleId = p.PayrollCycleId,
                Year = p.PayrollCycle.Year,
                Month = p.PayrollCycle.Month,
                PayslipNumber = p.PayslipNumber,
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

        return new GenericResponse<MyPayslipDetailsDto>
        {
            Success = true,
            Message = "Payslip details retrieved successfully",
            Data = payslip
        };
    }
}
