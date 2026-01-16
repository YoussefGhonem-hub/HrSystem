using ErrorOr;
using HrSystem.Application.Features.Lifecycle.OnboardingTasks.Queries.GetOnboardingTaskById;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Lifecycle.OnboardingTasks.Commands.UpdateOnboardingTask;

public record UpdateOnboardingTaskCommand(
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
) : IRequest<ErrorOr<GenericResponse<OnboardingTaskDto>>>;
