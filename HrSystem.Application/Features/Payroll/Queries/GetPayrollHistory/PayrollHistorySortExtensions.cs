using HrSystem.Domain.Entities.Payroll;

namespace HrSystem.Application.Features.Payroll.Queries.GetPayrollHistory;

public static class PayrollHistorySortExtensions
{
    public static IQueryable<Payslip> ApplySorting(
        this IQueryable<Payslip> query,
        string? sortBy,
        bool descending)
    {
        return (sortBy?.ToLower()) switch
        {
            "employeecode" => descending
                ? query.OrderByDescending(p => p.Employee.EmployeeCode)
                : query.OrderBy(p => p.Employee.EmployeeCode),
            "employeename" => descending
                ? query.OrderByDescending(p => p.Employee.FirstNameEn)
                : query.OrderBy(p => p.Employee.FirstNameEn),
            "department" => descending
                ? query.OrderByDescending(p => p.Employee.Department.NameEn)
                : query.OrderBy(p => p.Employee.Department.NameEn),
            "netsalary" => descending
                ? query.OrderByDescending(p => p.NetSalary)
                : query.OrderBy(p => p.NetSalary),
            "grosssalary" => descending
                ? query.OrderByDescending(p => p.GrossSalary)
                : query.OrderBy(p => p.GrossSalary),
            "month" => descending
                ? query.OrderByDescending(p => p.PayrollCycle.Year).ThenByDescending(p => p.PayrollCycle.Month)
                : query.OrderBy(p => p.PayrollCycle.Year).ThenBy(p => p.PayrollCycle.Month),
            "status" => descending
                ? query.OrderByDescending(p => p.IsPaid)
                : query.OrderBy(p => p.IsPaid),
            _ => query
                .OrderByDescending(p => p.PayrollCycle.Year)
                .ThenByDescending(p => p.PayrollCycle.Month)
                .ThenBy(p => p.Employee.EmployeeCode)
        };
    }
}
