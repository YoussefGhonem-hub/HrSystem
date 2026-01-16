using ErrorOr;
using HrSystem.Application.Features.Lifecycle.PolicyAcknowledgments.Queries.GetPolicyAcknowledgmentById;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Lifecycle.PolicyAcknowledgments.Commands.UpdatePolicyAcknowledgment;

public class UpdatePolicyAcknowledgmentCommandHandler : IRequestHandler<UpdatePolicyAcknowledgmentCommand, ErrorOr<GenericResponse<PolicyAcknowledgmentDto>>>
{
    private readonly ApplicationDbContext _context;

    public UpdatePolicyAcknowledgmentCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<PolicyAcknowledgmentDto>>> Handle(
        UpdatePolicyAcknowledgmentCommand request,
        CancellationToken cancellationToken)
    {
        var acknowledgment = await _context.PolicyAcknowledgments
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (acknowledgment == null)
        {
            return Error.NotFound(description: "Policy acknowledgment not found");
        }

        acknowledgment.PolicyName = request.PolicyName;
        acknowledgment.PolicyVersion = request.PolicyVersion;
        acknowledgment.AcknowledgedDate = request.AcknowledgedDate;
        acknowledgment.IsAcknowledged = request.IsAcknowledged;
        acknowledgment.DocumentPath = request.DocumentPath;
        acknowledgment.DocumentUrl = request.DocumentUrl;
        acknowledgment.EmployeeSignature = request.EmployeeSignature;
        acknowledgment.Notes = request.Notes;

        await _context.SaveChangesAsync(cancellationToken);

        var updatedAcknowledgment = await _context.PolicyAcknowledgments
            .Include(p => p.Employee)
            .FirstAsync(p => p.Id == acknowledgment.Id, cancellationToken);

        var dto = new PolicyAcknowledgmentDto
        {
            Id = updatedAcknowledgment.Id,
            EmployeeId = updatedAcknowledgment.EmployeeId,
            EmployeeName = updatedAcknowledgment.Employee?.FullNameEn,
            PolicyName = updatedAcknowledgment.PolicyName,
            PolicyVersion = updatedAcknowledgment.PolicyVersion,
            AcknowledgedDate = updatedAcknowledgment.AcknowledgedDate,
            IsAcknowledged = updatedAcknowledgment.IsAcknowledged,
            DocumentPath = updatedAcknowledgment.DocumentPath,
            DocumentUrl = updatedAcknowledgment.DocumentUrl,
            EmployeeSignature = updatedAcknowledgment.EmployeeSignature,
            Notes = updatedAcknowledgment.Notes
        };

        return new GenericResponse<PolicyAcknowledgmentDto>
        {
            Success = true,
            Message = "Policy acknowledgment updated successfully",
            Data = dto
        };
    }
}
