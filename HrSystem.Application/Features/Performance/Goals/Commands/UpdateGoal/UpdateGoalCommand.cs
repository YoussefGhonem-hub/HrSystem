using ErrorOr;
using HrSystem.Application.Features.Performance.Goals.Queries.GetGoalById;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Performance.Goals.Commands.UpdateGoal;

public record UpdateGoalCommand(
    Guid Id,
    string TitleAr,
    string TitleEn,
    string? DescriptionAr,
    string? DescriptionEn,
    DateTime StartDate,
    DateTime TargetDate,
    string Status,
    int Progress,
    string Priority,
    Guid? AssignedBy,
    string? CompletionNotes
) : IRequest<ErrorOr<GenericResponse<GoalDto>>>;
