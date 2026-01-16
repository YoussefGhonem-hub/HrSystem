using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Performance.Goals.Queries.GetGoalsList;

public record GetGoalsListQuery(
    Guid? EmployeeId = null,
    string? Status = null,
    string? Priority = null,
    DateTime? StartDateFrom = null,
    DateTime? StartDateTo = null,
    DateTime? TargetDateFrom = null,
    DateTime? TargetDateTo = null,
    string? SortBy = null,
    bool IsDescending = false,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<ErrorOr<GenericResponse<PagedResult<GoalListDto>>>>;
