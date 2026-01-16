using ErrorOr;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Performance.GoalPriorities.Commands.DeleteGoalPriority;

public record DeleteGoalPriorityCommand(Guid Id) : IRequest<ErrorOr<GenericResponse>>;
