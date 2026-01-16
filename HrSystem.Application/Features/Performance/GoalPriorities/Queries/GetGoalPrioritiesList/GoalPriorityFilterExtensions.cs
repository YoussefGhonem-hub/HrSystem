using HrSystem.Domain.Entities.Performance;
using System.Linq.Expressions;

namespace HrSystem.Application.Features.Performance.GoalPriorities.Queries.GetGoalPrioritiesList;

public static class GoalPriorityFilterExtensions
{
    public static IQueryable<GoalPriority> ApplyFilters(
        this IQueryable<GoalPriority> query,
        string? nameAr,
        string? nameEn,
        bool? isActive)
    {
        if (!string.IsNullOrWhiteSpace(nameAr))
        {
            query = query.Where(gp => gp.NameAr.Contains(nameAr));
        }

        if (!string.IsNullOrWhiteSpace(nameEn))
        {
            var search = nameEn.ToLower();
            query = query.Where(gp => gp.NameEn.ToLower().Contains(search));
        }

        if (isActive.HasValue)
        {
            query = query.Where(gp => gp.IsActive == isActive.Value);
        }

        return query;
    }

    public static IQueryable<GoalPriority> ApplySorting(this IQueryable<GoalPriority> query, string? sortBy, bool sortDescending)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            sortBy = "DisplayOrder";

        var parameter = Expression.Parameter(typeof(GoalPriority), "x");
        var property = typeof(GoalPriority).GetProperty(sortBy);

        if (property == null)
            return query.OrderBy(gp => gp.DisplayOrder);

        var propertyAccess = Expression.MakeMemberAccess(parameter, property);
        var orderByExpression = Expression.Lambda(propertyAccess, parameter);

        var methodName = sortDescending ? "OrderByDescending" : "OrderBy";
        var resultExpression = Expression.Call(
            typeof(Queryable),
            methodName,
            new[] { typeof(GoalPriority), property.PropertyType },
            query.Expression,
            Expression.Quote(orderByExpression));

        return query.Provider.CreateQuery<GoalPriority>(resultExpression);
    }

    public static IQueryable<GoalPriority> ApplyPaging(this IQueryable<GoalPriority> query, int pageNumber, int pageSize)
    {
        return query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }
}
