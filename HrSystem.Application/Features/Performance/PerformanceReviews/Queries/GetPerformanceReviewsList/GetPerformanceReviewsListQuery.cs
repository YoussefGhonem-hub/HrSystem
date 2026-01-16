using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Performance.PerformanceReviews.Queries.GetPerformanceReviewsList;

public record GetPerformanceReviewsListQuery(
    Guid? EmployeeId = null,
    Guid? ReviewerId = null,
    string? ReviewType = null,
    string? Status = null,
    DateTime? ReviewDateFrom = null,
    DateTime? ReviewDateTo = null,
    bool? EmployeeAcknowledged = null,
    string? SortBy = null,
    bool IsDescending = false,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<ErrorOr<GenericResponse<PagedResult<PerformanceReviewListDto>>>>;
