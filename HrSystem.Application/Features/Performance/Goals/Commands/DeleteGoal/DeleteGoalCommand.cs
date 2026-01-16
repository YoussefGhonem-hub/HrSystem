using ErrorOr;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Performance.Goals.Commands.DeleteGoal;

public record DeleteGoalCommand(Guid Id) : IRequest<ErrorOr<GenericResponse>>;
