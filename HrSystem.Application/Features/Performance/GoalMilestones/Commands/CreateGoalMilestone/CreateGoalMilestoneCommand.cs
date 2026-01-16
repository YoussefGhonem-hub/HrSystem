using ErrorOr;
using HrSystem.Application.Features.Performance.GoalMilestones.Common;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Performance.GoalMilestones.Commands.CreateGoalMilestone;

public record CreateGoalMilestoneCommand(
    Guid GoalId,
    string TitleAr,
    string TitleEn,
    DateTime DueDate,
    bool IsCompleted,
    DateTime? CompletionDate,
    string? Notes
) : IRequest<ErrorOr<GenericResponse<GoalMilestoneDto>>>;
