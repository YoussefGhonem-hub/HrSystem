using ErrorOr;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Performance.GoalStatuses.Commands.DeleteGoalStatus;

public record DeleteGoalStatusCommand(Guid Id) : IRequest<ErrorOr<GenericResponse>>;
