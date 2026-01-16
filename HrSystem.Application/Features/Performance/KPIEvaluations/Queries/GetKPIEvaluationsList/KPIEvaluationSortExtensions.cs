using HrSystem.Application.Common.PaginatedList;
using HrSystem.Domain.Entities.Performance;

namespace HrSystem.Application.Features.Performance.KPIEvaluations.Queries.GetKPIEvaluationsList;

public static class KPIEvaluationSortExtensions
{
    public static IQueryable<KPIEvaluation> ApplySorting(
        this IQueryable<KPIEvaluation> query,
        string? sortBy,
        bool sortDescending)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            sortBy = "rating";

        query = sortBy.ToLower() switch
        {
            "rating" => sortDescending
                ? query.OrderByDescending(ke => ke.Rating)
                : query.OrderBy(ke => ke.Rating),
            "weightedscore" => sortDescending
                ? query.OrderByDescending(ke => ke.WeightedScore)
                : query.OrderBy(ke => ke.WeightedScore),
            "createdate" => sortDescending
                ? query.OrderByDescending(ke => ke.CreatedDate)
                : query.OrderBy(ke => ke.CreatedDate),
            _ => query.OrderByDescending(ke => ke.Rating)
        };

        return query;
    }
}
