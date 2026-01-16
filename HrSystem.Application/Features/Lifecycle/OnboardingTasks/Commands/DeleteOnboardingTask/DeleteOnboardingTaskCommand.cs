using ErrorOr;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Lifecycle.OnboardingTasks.Commands.DeleteOnboardingTask;

public record DeleteOnboardingTaskCommand(Guid Id) : IRequest<ErrorOr<GenericResponse>>;
