using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Performance.GoalMilestones.Queries.GetGoalMilestonesList;

public record GetGoalMilestonesListQuery(
    int PageNumber = 1,
    int PageSize = 10,
    Guid? GoalId = null,
    bool? IsCompleted = null,
    DateTime? DueDateFrom = null,
    DateTime? DueDateTo = null,
    string? SortBy = null,
    bool SortDescending = false
) : IRequest<ErrorOr<GenericResponse<PagedResult<GoalMilestoneListDto>>>>;
