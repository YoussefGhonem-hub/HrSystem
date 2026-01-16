using ErrorOr;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Performance.GoalMilestones.Commands.DeleteGoalMilestone;

public record DeleteGoalMilestoneCommand(Guid Id) : IRequest<ErrorOr<GenericResponse>>;
