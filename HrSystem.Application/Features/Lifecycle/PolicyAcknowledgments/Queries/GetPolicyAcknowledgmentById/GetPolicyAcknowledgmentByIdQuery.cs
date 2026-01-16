using ErrorOr;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Lifecycle.PolicyAcknowledgments.Queries.GetPolicyAcknowledgmentById;

public record GetPolicyAcknowledgmentByIdQuery(Guid Id) : IRequest<ErrorOr<GenericResponse<PolicyAcknowledgmentDto>>>;
