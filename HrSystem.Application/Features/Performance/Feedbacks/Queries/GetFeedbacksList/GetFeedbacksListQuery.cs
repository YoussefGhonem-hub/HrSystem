using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Performance.Feedbacks.Queries.GetFeedbacksList;

public record GetFeedbacksListQuery(
    int PageNumber = 1,
    int PageSize = 10,
    Guid? PerformanceReviewId = null,
    Guid? ProvidedBy = null,
    string? FeedbackType = null,
    bool? IsAnonymous = null,
    decimal? RatingMin = null,
    decimal? RatingMax = null,
    string? SortBy = null,
    bool SortDescending = false
) : IRequest<ErrorOr<GenericResponse<PagedResult<FeedbackListDto>>>>;
