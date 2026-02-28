using System.Globalization;
using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Payroll.Queries.GetPayrollHistory;

public record GetPayrollHistoryQuery(
    Guid? EmployeeId = null,
    int? Year = null,
    int PageNumber = 1,
    int PageSize = 12
) : IRequest<ErrorOr<GenericResponse<PagedResult<PayrollHistoryListItemDto>>>>;

public class GetPayrollHistoryQueryHandler
    : IRequestHandler<GetPayrollHistoryQuery, ErrorOr<GenericResponse<PagedResult<PayrollHistoryListItemDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetPayrollHistoryQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<PagedResult<PayrollHistoryListItemDto>>>> Handle(
        GetPayrollHistoryQuery request,
        CancellationToken cancellationToken)
    {
        // Resolve target employee
        var employeeId = request.EmployeeId;

        if (!employeeId.HasValue || employeeId.Value == Guid.Empty)
        {
            employeeId = CurrentUser.EmployeeId;

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
        }

        if (!employeeId.HasValue || employeeId.Value == Guid.Empty)
        {
            return Error.Unauthorized("PayrollHistory.Unauthorized",
                "Current user is not linked to an employee");
        }

        // Get the employee's salary currency
        var currency = await _context.Salaries
            .Where(s => s.EmployeeId == employeeId.Value && s.IsCurrent)
            .OrderByDescending(s => s.EffectiveDate)
            .Select(s => s.Currency)
            .FirstOrDefaultAsync(cancellationToken) ?? "EGP";

        // Build query
        var query = _context.Payslips
            .Include(p => p.PayrollCycle)
            .Where(p => !p.IsDeleted && p.EmployeeId == employeeId.Value)
            .AsQueryable();

        if (request.Year.HasValue)
        {
            query = query.Where(p => p.PayrollCycle.Year == request.Year.Value);
        }

        var payslipsQuery = query
            .OrderByDescending(p => p.PayrollCycle.Year)
            .ThenByDescending(p => p.PayrollCycle.Month)
            .ThenByDescending(p => p.GeneratedDate)
            .Select(p => new PayrollHistoryListItemDto
            {
                PayslipId = p.Id,
                PayrollCycleId = p.PayrollCycleId,
                Year = p.PayrollCycle.Year,
                Month = p.PayrollCycle.Month,
                MonthName = CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(p.PayrollCycle.Month),
                PeriodLabel = CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(p.PayrollCycle.Month)
                              + " " + p.PayrollCycle.Year,
                BasicSalary = p.BasicSalary,
                TotalAllowances = p.TotalAllowances,
                GrossSalary = p.GrossSalary,
                TotalDeductions = p.TotalDeductions,
                IncomeTax = p.IncomeTax,
                SocialInsuranceEmployee = p.SocialInsuranceEmployee,
                OvertimeAmount = p.OvertimeAmount,
                BonusAmount = p.BonusAmount,
                LeaveDeductions = p.LeaveDeductions,
                UnpaidLeaveDays = p.UnpaidLeaveDays,
                NetSalary = p.NetSalary,
                Currency = currency,
                TotalWorkingDays = p.TotalWorkingDays,
                ActualWorkingDays = p.ActualWorkingDays,
                AbsentDays = p.AbsentDays,
                IsPaid = p.IsPaid,
                PaidDate = p.PaidDate,
                GeneratedDate = p.GeneratedDate,
                Status = p.IsPaid ? "Paid" : (p.GeneratedDate.HasValue ? "Generated" : "Draft"),
                PdfFileUrl = p.PdfFileUrl
            });

        var pagedResult = await payslipsQuery.ToPagedResultAsync(
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        return GenericResponse<PagedResult<PayrollHistoryListItemDto>>.SuccessResult(
            pagedResult,
            "Payroll history retrieved successfully");
    }
}
