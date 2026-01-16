using ErrorOr;
using HrSystem.Application.Features.Performance.GoalPriorities.Common;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Performance.GoalPriorities.Queries.GetGoalPriorityById;

public record GetGoalPriorityByIdQuery(Guid Id) : IRequest<ErrorOr<GenericResponse<GoalPriorityDto>>>;
