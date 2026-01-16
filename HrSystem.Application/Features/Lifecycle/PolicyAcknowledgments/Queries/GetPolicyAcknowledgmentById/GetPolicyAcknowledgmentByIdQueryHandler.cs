using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Lifecycle.PolicyAcknowledgments.Queries.GetPolicyAcknowledgmentById;

public class GetPolicyAcknowledgmentByIdQueryHandler : IRequestHandler<GetPolicyAcknowledgmentByIdQuery, ErrorOr<GenericResponse<PolicyAcknowledgmentDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetPolicyAcknowledgmentByIdQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<PolicyAcknowledgmentDto>>> Handle(
        GetPolicyAcknowledgmentByIdQuery request,
        CancellationToken cancellationToken)
    {
        var acknowledgment = await _context.PolicyAcknowledgments
            .Include(p => p.Employee)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (acknowledgment == null)
        {
            return Error.NotFound(description: "Policy acknowledgment not found");
        }

        var dto = new PolicyAcknowledgmentDto
        {
            Id = acknowledgment.Id,
            EmployeeId = acknowledgment.EmployeeId,
            EmployeeName = acknowledgment.Employee?.FullNameEn,
            PolicyName = acknowledgment.PolicyName,
            PolicyVersion = acknowledgment.PolicyVersion,
            AcknowledgedDate = acknowledgment.AcknowledgedDate,
            IsAcknowledged = acknowledgment.IsAcknowledged,
            DocumentPath = acknowledgment.DocumentPath,
            DocumentUrl = acknowledgment.DocumentUrl,
            EmployeeSignature = acknowledgment.EmployeeSignature,
            Notes = acknowledgment.Notes
        };

        return new GenericResponse<PolicyAcknowledgmentDto>
        {
            Success = true,
            Data = dto
        };
    }
}
