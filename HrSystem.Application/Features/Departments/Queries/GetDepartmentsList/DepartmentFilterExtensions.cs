using HrSystem.Domain.Entities.Employee;
using System.Linq.Expressions;

namespace HrSystem.Application.Features.Departments.Queries.GetDepartmentsList;

public static class DepartmentFilterExtensions
{
    public static IQueryable<Department> ApplyFilters(
        this IQueryable<Department> query,
        string? searchTerm,
        Guid? branchId)
    {
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var search = searchTerm.ToLower();
            query = query.Where(d =>
                d.NameEn.ToLower().Contains(search) ||
                d.NameAr.Contains(searchTerm));
        }

        if (branchId.HasValue)
            query = query.Where(d => d.BranchId == branchId.Value);

        return query;
    }

    public static IQueryable<Department> ApplySorting(this IQueryable<Department> query, string? sortBy, bool sortDescending)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            sortBy = "CreatedDate";

        var parameter = Expression.Parameter(typeof(Department), "x");
        var property = typeof(Department).GetProperty(sortBy);

        if (property == null)
            return query.OrderByDescending(d => d.CreatedDate);

        var propertyAccess = Expression.MakeMemberAccess(parameter, property);
        var orderByExpression = Expression.Lambda(propertyAccess, parameter);

        var methodName = sortDescending ? "OrderByDescending" : "OrderBy";
        var resultExpression = Expression.Call(
            typeof(Queryable),
            methodName,
            new[] { typeof(Department), property.PropertyType },
            query.Expression,
            Expression.Quote(orderByExpression));

        return query.Provider.CreateQuery<Department>(resultExpression);
    }

    public static IQueryable<Department> ApplyPaging(this IQueryable<Department> query, int pageNumber, int pageSize)
    {
        return query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }
}
