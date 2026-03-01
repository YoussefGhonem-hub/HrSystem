using HrSystem.Domain.Entities.Payroll;

namespace HrSystem.Application.Features.Payroll.Queries.GetPayrollHistory;

public static class PayrollHistoryFilterExtensions
{
    public static IQueryable<Payslip> ApplyFilters(
        this IQueryable<Payslip> query,
        int? year,
        int? month,
        Guid? employeeId,
        Guid? departmentId,
        bool? isPaid,
        string? searchTerm)
    {
        if (year.HasValue)
        {
            query = query.Where(p => p.PayrollCycle.Year == year.Value);
        }

        if (month.HasValue)
        {
            query = query.Where(p => p.PayrollCycle.Month == month.Value);
        }

        if (employeeId.HasValue)
        {
            query = query.Where(p => p.EmployeeId == employeeId.Value);
        }

        if (departmentId.HasValue)
        {
            query = query.Where(p => p.Employee.DepartmentId == departmentId.Value);
        }

        if (isPaid.HasValue)
        {
            query = query.Where(p => p.IsPaid == isPaid.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(p =>
                p.Employee.EmployeeCode.ToLower().Contains(term) ||
                p.Employee.FirstNameEn.ToLower().Contains(term) ||
                p.Employee.LastNameEn.ToLower().Contains(term) ||
                p.Employee.FirstNameAr.Contains(term) ||
                p.Employee.LastNameAr.Contains(term) ||
                p.PayslipNumber.ToLower().Contains(term));
        }

        return query;
    }
}
