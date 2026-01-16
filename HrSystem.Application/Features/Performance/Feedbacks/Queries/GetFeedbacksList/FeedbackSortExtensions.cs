using HrSystem.Application.Common.PaginatedList;
using HrSystem.Domain.Entities.Performance;

namespace HrSystem.Application.Features.Performance.Feedbacks.Queries.GetFeedbacksList;

public static class FeedbackSortExtensions
{
    public static IQueryable<Feedback> ApplySorting(
        this IQueryable<Feedback> query,
        string? sortBy,
        bool sortDescending)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            sortBy = "createdate";

        query = sortBy.ToLower() switch
        {
            "rating" => sortDescending
                ? query.OrderByDescending(f => f.Rating)
                : query.OrderBy(f => f.Rating),
            "feedbacktype" => sortDescending
                ? query.OrderByDescending(f => f.FeedbackType)
                : query.OrderBy(f => f.FeedbackType),
            "createdate" => sortDescending
                ? query.OrderByDescending(f => f.CreatedDate)
                : query.OrderBy(f => f.CreatedDate),
            _ => query.OrderByDescending(f => f.CreatedDate)
        };

        return query;
    }
}
