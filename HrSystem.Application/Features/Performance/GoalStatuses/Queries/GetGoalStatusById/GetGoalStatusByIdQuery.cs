using ErrorOr;
using HrSystem.Application.Features.Performance.GoalStatuses.Common;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Performance.GoalStatuses.Queries.GetGoalStatusById;

public record GetGoalStatusByIdQuery(Guid Id) : IRequest<ErrorOr<GenericResponse<GoalStatusDto>>>;
