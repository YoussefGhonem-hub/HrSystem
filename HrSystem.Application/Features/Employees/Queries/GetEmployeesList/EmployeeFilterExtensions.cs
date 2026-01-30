using HrSystem.Domain.Entities.Employee;
using System.Linq.Expressions;

namespace HrSystem.Application.Features.Employees.Queries.GetEmployeesList;

public static class EmployeeFilterExtensions
{
    public static IQueryable<Employee> ApplyFilters(
        this IQueryable<Employee> query,
        string? searchTerm,
        Guid? statusId,
        Guid? departmentId,
        Guid? branchId,
        Guid? jobTitleId,
        Guid? managerId)
    {
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var search = searchTerm.ToLower();
            query = query.Where(e =>
                e.FirstNameEn.ToLower().Contains(search) ||
                e.LastNameEn.ToLower().Contains(search) ||
                e.FirstNameAr.Contains(searchTerm) ||
                e.LastNameAr.Contains(searchTerm) ||
                e.EmployeeCode.ToLower().Contains(search) ||
                e.Email.ToLower().Contains(search));
        }

        if (statusId.HasValue)
            query = query.Where(e => e.StatusId == statusId.Value);

        if (departmentId.HasValue)
            query = query.Where(e => e.DepartmentId == departmentId.Value);

        if (branchId.HasValue)
            query = query.Where(e => e.BranchId == branchId.Value);

        if (jobTitleId.HasValue)
            query = query.Where(e => e.JobTitleId == jobTitleId.Value);

        if (managerId.HasValue)
            query = query.Where(e => e.DirectManagerId == managerId.Value);

        return query;
    }

    public static IQueryable<Employee> ApplySorting(this IQueryable<Employee> query, string? sortBy, bool sortDescending)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            sortBy = nameof(Employee.CreatedDate);
        else
            sortBy = sortBy.Trim();

        // Handle computed names that are not mapped to the database
        if (string.Equals(sortBy, nameof(Employee.FullNameEn), StringComparison.OrdinalIgnoreCase))
        {
            return sortDescending
                ? query.OrderByDescending(e => e.FirstNameEn).ThenByDescending(e => e.LastNameEn)
                : query.OrderBy(e => e.FirstNameEn).ThenBy(e => e.LastNameEn);
        }

        if (string.Equals(sortBy, nameof(Employee.FullNameAr), StringComparison.OrdinalIgnoreCase))
        {
            return sortDescending
                ? query.OrderByDescending(e => e.FirstNameAr).ThenByDescending(e => e.LastNameAr)
                : query.OrderBy(e => e.FirstNameAr).ThenBy(e => e.LastNameAr);
        }

        var parameter = Expression.Parameter(typeof(Employee), "x");
        var property = typeof(Employee).GetProperty(sortBy);

        if (property == null)
            return query.OrderByDescending(e => e.CreatedDate);

        var propertyAccess = Expression.MakeMemberAccess(parameter, property);
        var orderByExpression = Expression.Lambda(propertyAccess, parameter);

        var methodName = sortDescending ? "OrderByDescending" : "OrderBy";
        var resultExpression = Expression.Call(
            typeof(Queryable),
            methodName,
            new[] { typeof(Employee), property.PropertyType },
            query.Expression,
            Expression.Quote(orderByExpression));

        return query.Provider.CreateQuery<Employee>(resultExpression);
    }

    public static IQueryable<Employee> ApplyPaging(this IQueryable<Employee> query, int pageNumber, int pageSize)
    {
        return query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }
}
