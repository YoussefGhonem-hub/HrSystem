using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Lifecycle.OnboardingTasks.Queries.GetOnboardingTasksList;

public record GetOnboardingTasksListQuery(
    Guid? EmployeeId = null,
    bool? IsCompleted = null,
    string? Category = null,
    DateTime? DueDateFrom = null,
    DateTime? DueDateTo = null,
    string? SortBy = null,
    bool IsDescending = false,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<ErrorOr<GenericResponse<PagedResult<OnboardingTaskListDto>>>>;
