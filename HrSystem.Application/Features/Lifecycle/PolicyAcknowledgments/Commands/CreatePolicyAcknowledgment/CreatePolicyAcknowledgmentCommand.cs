using ErrorOr;
using HrSystem.Application.Features.Lifecycle.PolicyAcknowledgments.Queries.GetPolicyAcknowledgmentById;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Lifecycle.PolicyAcknowledgments.Commands.CreatePolicyAcknowledgment;

public record CreatePolicyAcknowledgmentCommand(
    Guid EmployeeId,
    string PolicyName,
    string PolicyVersion,
    DateTime AcknowledgedDate,
    string? DocumentPath,
    string? DocumentUrl,
    string? EmployeeSignature,
    string? Notes
) : IRequest<ErrorOr<GenericResponse<PolicyAcknowledgmentDto>>>;

public class CreatePolicyAcknowledgmentCommandHandler : IRequestHandler<CreatePolicyAcknowledgmentCommand, ErrorOr<GenericResponse<PolicyAcknowledgmentDto>>>
{
    private readonly ApplicationDbContext _context;

    public CreatePolicyAcknowledgmentCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<PolicyAcknowledgmentDto>>> Handle(
        CreatePolicyAcknowledgmentCommand request,
        CancellationToken cancellationToken)
    {
        var acknowledgment = new Domain.Entities.Lifecycle.PolicyAcknowledgment
        {
            EmployeeId = request.EmployeeId,
            PolicyName = request.PolicyName,
            PolicyVersion = request.PolicyVersion,
            AcknowledgedDate = request.AcknowledgedDate,
            DocumentPath = request.DocumentPath,
            DocumentUrl = request.DocumentUrl,
            EmployeeSignature = request.EmployeeSignature,
            Notes = request.Notes,
            IsAcknowledged = true,
            TenantId = Guid.NewGuid()
        };

        _context.PolicyAcknowledgments.Add(acknowledgment);
        await _context.SaveChangesAsync(cancellationToken);

        var createdAcknowledgment = await _context.PolicyAcknowledgments
            .Include(p => p.Employee)
            .FirstAsync(p => p.Id == acknowledgment.Id, cancellationToken);

        var dto = new PolicyAcknowledgmentDto
        {
            Id = createdAcknowledgment.Id,
            EmployeeId = createdAcknowledgment.EmployeeId,
            EmployeeName = createdAcknowledgment.Employee?.FullNameEn,
            PolicyName = createdAcknowledgment.PolicyName,
            PolicyVersion = createdAcknowledgment.PolicyVersion,
            AcknowledgedDate = createdAcknowledgment.AcknowledgedDate,
            IsAcknowledged = createdAcknowledgment.IsAcknowledged,
            DocumentPath = createdAcknowledgment.DocumentPath,
            DocumentUrl = createdAcknowledgment.DocumentUrl,
            EmployeeSignature = createdAcknowledgment.EmployeeSignature,
            Notes = createdAcknowledgment.Notes
        };

        return new GenericResponse<PolicyAcknowledgmentDto>
        {
            Success = true,
            Message = "Policy acknowledgment created successfully",
            Data = dto
        };
    }
}
