using ErrorOr;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Lifecycle.OffboardingTasks.Queries.GetOffboardingTaskById;

public record GetOffboardingTaskByIdQuery(Guid Id) : IRequest<ErrorOr<GenericResponse<OffboardingTaskDto>>>;
