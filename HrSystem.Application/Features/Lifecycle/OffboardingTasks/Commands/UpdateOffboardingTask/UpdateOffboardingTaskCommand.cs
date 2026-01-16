using ErrorOr;
using HrSystem.Application.Features.Lifecycle.OffboardingTasks.Queries.GetOffboardingTaskById;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Lifecycle.OffboardingTasks.Commands.UpdateOffboardingTask;

public record UpdateOffboardingTaskCommand(
    Guid Id,
    string TaskNameAr,
    string TaskNameEn,
    string? DescriptionAr,
    string? DescriptionEn,
    int Sequence,
    DateTime DueDate,
    bool IsCompleted,
    Guid? AssignedTo,
    string Category,
    string? Notes
) : IRequest<ErrorOr<GenericResponse<OffboardingTaskDto>>>;
