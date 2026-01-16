using HrSystem.Domain.Entities.Performance;

namespace HrSystem.Application.Features.Performance.Feedbacks.Queries.GetFeedbacksList;

public static class FeedbackFilterExtensions
{
    public static IQueryable<Feedback> ApplyFilters(
        this IQueryable<Feedback> query,
        Guid? performanceReviewId,
        Guid? providedBy,
        string? feedbackType,
        bool? isAnonymous,
        decimal? ratingMin,
        decimal? ratingMax)
    {
        if (performanceReviewId.HasValue)
            query = query.Where(f => f.PerformanceReviewId == performanceReviewId.Value);

        if (providedBy.HasValue)
            query = query.Where(f => f.ProvidedBy == providedBy.Value);

        if (!string.IsNullOrWhiteSpace(feedbackType))
            query = query.Where(f => f.FeedbackType == feedbackType);

        if (isAnonymous.HasValue)
            query = query.Where(f => f.IsAnonymous == isAnonymous.Value);

        if (ratingMin.HasValue)
            query = query.Where(f => f.Rating >= ratingMin.Value);

        if (ratingMax.HasValue)
            query = query.Where(f => f.Rating <= ratingMax.Value);

        return query;
    }

    public static IQueryable<Feedback> ApplyPaging(
        this IQueryable<Feedback> query,
        int pageNumber,
        int pageSize)
    {
        return query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }
}
