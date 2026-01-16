using HrSystem.Domain.Entities.Performance;
using System.Linq.Expressions;

namespace HrSystem.Application.Features.Performance.GoalStatuses.Queries.GetGoalStatusesList;

public static class GoalStatusFilterExtensions
{
    public static IQueryable<GoalStatus> ApplyFilters(
        this IQueryable<GoalStatus> query,
        string? nameAr,
        string? nameEn,
        bool? isActive)
    {
        if (!string.IsNullOrWhiteSpace(nameAr))
        {
            query = query.Where(gs => gs.NameAr.Contains(nameAr));
        }

        if (!string.IsNullOrWhiteSpace(nameEn))
        {
            var search = nameEn.ToLower();
            query = query.Where(gs => gs.NameEn.ToLower().Contains(search));
        }

        if (isActive.HasValue)
        {
            query = query.Where(gs => gs.IsActive == isActive.Value);
        }

        return query;
    }

    public static IQueryable<GoalStatus> ApplySorting(this IQueryable<GoalStatus> query, string? sortBy, bool sortDescending)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            sortBy = "DisplayOrder";

        var parameter = Expression.Parameter(typeof(GoalStatus), "x");
        var property = typeof(GoalStatus).GetProperty(sortBy);

        if (property == null)
            return query.OrderBy(gs => gs.DisplayOrder);

        var propertyAccess = Expression.MakeMemberAccess(parameter, property);
        var orderByExpression = Expression.Lambda(propertyAccess, parameter);

        var methodName = sortDescending ? "OrderByDescending" : "OrderBy";
        var resultExpression = Expression.Call(
            typeof(Queryable),
            methodName,
            new[] { typeof(GoalStatus), property.PropertyType },
            query.Expression,
            Expression.Quote(orderByExpression));

        return query.Provider.CreateQuery<GoalStatus>(resultExpression);
    }

    public static IQueryable<GoalStatus> ApplyPaging(this IQueryable<GoalStatus> query, int pageNumber, int pageSize)
    {
        return query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }
}
