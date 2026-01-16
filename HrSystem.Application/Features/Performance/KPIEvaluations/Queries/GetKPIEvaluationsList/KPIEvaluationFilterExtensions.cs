using HrSystem.Domain.Entities.Performance;

namespace HrSystem.Application.Features.Performance.KPIEvaluations.Queries.GetKPIEvaluationsList;

public static class KPIEvaluationFilterExtensions
{
    public static IQueryable<KPIEvaluation> ApplyFilters(
        this IQueryable<KPIEvaluation> query,
        Guid? performanceReviewId,
        Guid? kpiId,
        decimal? ratingMin,
        decimal? ratingMax)
    {
        if (performanceReviewId.HasValue)
            query = query.Where(ke => ke.PerformanceReviewId == performanceReviewId.Value);

        if (kpiId.HasValue)
            query = query.Where(ke => ke.KPIId == kpiId.Value);

        if (ratingMin.HasValue)
            query = query.Where(ke => ke.Rating >= ratingMin.Value);

        if (ratingMax.HasValue)
            query = query.Where(ke => ke.Rating <= ratingMax.Value);

        return query;
    }

    public static IQueryable<KPIEvaluation> ApplyPaging(
        this IQueryable<KPIEvaluation> query,
        int pageNumber,
        int pageSize)
    {
        return query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }
}
