using ErrorOr;
using HrSystem.Application.Features.Performance.GoalStatuses.Common;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Performance.GoalStatuses.Commands.UpdateGoalStatus;

public record UpdateGoalStatusCommand(
    Guid Id,
    string NameAr,
    string NameEn,
    string? DescriptionAr,
    string? DescriptionEn,
    int DisplayOrder,
    bool IsActive
) : IRequest<ErrorOr<GenericResponse<GoalStatusDto>>>;
