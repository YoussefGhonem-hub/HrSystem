using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Performance.KPIEvaluations.Queries.GetKPIEvaluationsList;

public record GetKPIEvaluationsListQuery(
    int PageNumber = 1,
    int PageSize = 10,
    Guid? PerformanceReviewId = null,
    Guid? KPIId = null,
    decimal? RatingMin = null,
    decimal? RatingMax = null,
    string? SortBy = null,
    bool SortDescending = false
) : IRequest<ErrorOr<GenericResponse<PagedResult<KPIEvaluationListDto>>>>;
