using ErrorOr;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Lifecycle.PolicyAcknowledgments.Commands.DeletePolicyAcknowledgment;

public record DeletePolicyAcknowledgmentCommand(Guid Id) : IRequest<ErrorOr<GenericResponse>>;
