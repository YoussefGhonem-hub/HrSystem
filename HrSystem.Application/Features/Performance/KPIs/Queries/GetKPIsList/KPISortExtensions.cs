using HrSystem.Application.Common.PaginatedList;
using HrSystem.Domain.Entities.Performance;

namespace HrSystem.Application.Features.Performance.KPIs.Queries.GetKPIsList;

public static class KPISortExtensions
{
    public static IQueryable<KPI> ApplySorting(
        this IQueryable<KPI> query,
        string? sortBy,
        bool sortDescending)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            sortBy = "name";

        query = sortBy.ToLower() switch
        {
            "name" => sortDescending
                ? query.OrderByDescending(k => k.NameEn)
                : query.OrderBy(k => k.NameEn),
            "namear" => sortDescending
                ? query.OrderByDescending(k => k.NameAr)
                : query.OrderBy(k => k.NameAr),
            "category" => sortDescending
                ? query.OrderByDescending(k => k.Category)
                : query.OrderBy(k => k.Category),
            "weight" => sortDescending
                ? query.OrderByDescending(k => k.Weight)
                : query.OrderBy(k => k.Weight),
            "createdate" => sortDescending
                ? query.OrderByDescending(k => k.CreatedDate)
                : query.OrderBy(k => k.CreatedDate),
            _ => query.OrderBy(k => k.NameEn)
        };

        return query;
    }
}
