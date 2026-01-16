using HrSystem.Domain.Entities.Employee;
using System.Linq.Expressions;

namespace HrSystem.Application.Features.JobTitles.Queries.GetJobTitlesList;

public static class JobTitleFilterExtensions
{
    public static IQueryable<JobTitle> ApplyFilters(
        this IQueryable<JobTitle> query,
        string? searchTerm)
    {
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var search = searchTerm.ToLower();
            query = query.Where(j =>
                j.TitleEn.ToLower().Contains(search) ||
                j.TitleAr.Contains(searchTerm));
        }

        return query;
    }

    public static IQueryable<JobTitle> ApplySorting(this IQueryable<JobTitle> query, string? sortBy, bool sortDescending)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            sortBy = "Level";

        var parameter = Expression.Parameter(typeof(JobTitle), "x");
        var property = typeof(JobTitle).GetProperty(sortBy);

        if (property == null)
            return query.OrderBy(j => j.Level);

        var propertyAccess = Expression.MakeMemberAccess(parameter, property);
        var orderByExpression = Expression.Lambda(propertyAccess, parameter);

        var methodName = sortDescending ? "OrderByDescending" : "OrderBy";
        var resultExpression = Expression.Call(
            typeof(Queryable),
            methodName,
            new[] { typeof(JobTitle), property.PropertyType },
            query.Expression,
            Expression.Quote(orderByExpression));

        return query.Provider.CreateQuery<JobTitle>(resultExpression);
    }

    public static IQueryable<JobTitle> ApplyPaging(this IQueryable<JobTitle> query, int pageNumber, int pageSize)
    {
        return query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }
}
