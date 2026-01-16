using ErrorOr;
using HrSystem.Application.Features.Lifecycle.PolicyAcknowledgments.Queries.GetPolicyAcknowledgmentById;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Lifecycle.PolicyAcknowledgments.Commands.UpdatePolicyAcknowledgment;

public record UpdatePolicyAcknowledgmentCommand(
    Guid Id,
    string PolicyName,
    string PolicyVersion,
    DateTime AcknowledgedDate,
    bool IsAcknowledged,
    string? DocumentPath,
    string? DocumentUrl,
    string? EmployeeSignature,
    string? Notes
) : IRequest<ErrorOr<GenericResponse<PolicyAcknowledgmentDto>>>;
