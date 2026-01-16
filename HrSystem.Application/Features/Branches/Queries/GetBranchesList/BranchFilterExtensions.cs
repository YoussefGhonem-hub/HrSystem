using HrSystem.Domain.Entities.Organization;
using HrSystem.Domain.Enums;
using System.Linq.Expressions;

namespace HrSystem.Application.Features.Branches.Queries.GetBranchesList;

public static class BranchFilterExtensions
{
    public static IQueryable<Branch> ApplyFilters(
        this IQueryable<Branch> query,
        string? searchTerm,
        Country? country,
        bool? isActive)
    {
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var search = searchTerm.ToLower();
            query = query.Where(b =>
                b.NameEn.ToLower().Contains(search) ||
                b.NameAr.Contains(searchTerm) ||
                b.Code.ToLower().Contains(search));
        }

        if (country.HasValue)
            query = query.Where(b => b.Country == country.Value);

        if (isActive.HasValue)
            query = query.Where(b => b.IsActive == isActive.Value);

        return query;
    }

    public static IQueryable<Branch> ApplySorting(this IQueryable<Branch> query, string? sortBy, bool sortDescending)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            sortBy = "Code";

        var parameter = Expression.Parameter(typeof(Branch), "x");
        var property = typeof(Branch).GetProperty(sortBy);

        if (property == null)
            return query.OrderBy(b => b.Code);

        var propertyAccess = Expression.MakeMemberAccess(parameter, property);
        var orderByExpression = Expression.Lambda(propertyAccess, parameter);

        var methodName = sortDescending ? "OrderByDescending" : "OrderBy";
        var resultExpression = Expression.Call(
            typeof(Queryable),
            methodName,
            new[] { typeof(Branch), property.PropertyType },
            query.Expression,
            Expression.Quote(orderByExpression));

        return query.Provider.CreateQuery<Branch>(resultExpression);
    }

    public static IQueryable<Branch> ApplyPaging(this IQueryable<Branch> query, int pageNumber, int pageSize)
    {
        return query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }
}
