using ErrorOr;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Lifecycle.OffboardingTasks.Commands.DeleteOffboardingTask;

public record DeleteOffboardingTaskCommand(Guid Id) : IRequest<ErrorOr<GenericResponse>>;
