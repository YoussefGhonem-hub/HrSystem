using ErrorOr;
using HrSystem.Application.Features.Performance.GoalMilestones.Common;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Performance.GoalMilestones.Commands.UpdateGoalMilestone;

public record UpdateGoalMilestoneCommand(
    Guid Id,
    string TitleAr,
    string TitleEn,
    DateTime DueDate,
    bool IsCompleted,
    DateTime? CompletionDate,
    string? Notes
) : IRequest<ErrorOr<GenericResponse<GoalMilestoneDto>>>;
