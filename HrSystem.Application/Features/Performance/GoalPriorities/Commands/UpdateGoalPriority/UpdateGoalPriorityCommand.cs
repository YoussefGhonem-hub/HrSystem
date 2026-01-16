using ErrorOr;
using HrSystem.Application.Features.Performance.GoalPriorities.Common;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Performance.GoalPriorities.Commands.UpdateGoalPriority;

public record UpdateGoalPriorityCommand(
    Guid Id,
    string NameAr,
    string NameEn,
    string? DescriptionAr,
    string? DescriptionEn,
    int DisplayOrder,
    bool IsActive
) : IRequest<ErrorOr<GenericResponse<GoalPriorityDto>>>;
