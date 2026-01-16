using ErrorOr;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Lifecycle.OnboardingTasks.Queries.GetOnboardingTaskById;

public record GetOnboardingTaskByIdQuery(Guid Id) : IRequest<ErrorOr<GenericResponse<OnboardingTaskDto>>>;
