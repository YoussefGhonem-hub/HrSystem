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
    int? Year = null,
    int? Month = null,
    Guid? EmployeeId = null,
    Guid? DepartmentId = null,
    bool? IsPaid = null,
    string? SearchTerm = null,
    string? SortBy = "RequestedDate",
    bool SortDescending = true,
    int PageNumber = 1,
    int PageSize = 10
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
        // Resolve EmployeeId from CurrentUser when not provided
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

        // Build base query
        var query = _context.Payslips
            .Include(p => p.PayrollCycle)
            .Include(p => p.Employee)
                .ThenInclude(e => e.Department)
            .Where(p => !p.IsDeleted)
            .AsQueryable();

        // Apply filters & sorting via extension methods
        query = query
            .ApplyFilters(
                request.Year,
                request.Month,
                employeeId,
                request.DepartmentId,
                request.IsPaid,
                request.SearchTerm)
            .ApplySorting(request.SortBy, request.SortDescending);

        // Projection + Pagination
        var projectedQuery = query
            .Select(p => new PayrollHistoryListItemDto
            {
                PayslipId = p.Id,
                PayrollCycleId = p.PayrollCycleId,
                EmployeeId = p.EmployeeId,
                EmployeeCode = p.Employee.EmployeeCode,
                EmployeeNameEn = p.Employee.FullNameEn,
                EmployeeNameAr = p.Employee.FullNameAr,
                DepartmentNameEn = p.Employee.Department != null ? p.Employee.Department.NameEn : "No Department",
                DepartmentNameAr = p.Employee.Department != null ? p.Employee.Department.NameAr : "No Department",
                ProfilePictureUrl = p.Employee.ProfilePictureUrl,
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
                Currency = "EGP",
                TotalWorkingDays = p.TotalWorkingDays,
                ActualWorkingDays = p.ActualWorkingDays,
                AbsentDays = p.AbsentDays,
                IsPaid = p.IsPaid,
                PaidDate = p.PaidDate,
                GeneratedDate = p.GeneratedDate,
                Status = p.IsPaid ? "Paid" : (p.GeneratedDate.HasValue ? "Generated" : "Draft"),
                PdfFileUrl = p.PdfFileUrl
            });

        var pagedResult = await projectedQuery.ToPagedResultAsync(
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        return GenericResponse<PagedResult<PayrollHistoryListItemDto>>.SuccessResult(
            pagedResult,
            "Payroll history retrieved successfully");
    }
}
