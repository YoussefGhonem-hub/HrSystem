using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Lifecycle.PolicyAcknowledgments.Commands.DeletePolicyAcknowledgment;

public class DeletePolicyAcknowledgmentCommandHandler : IRequestHandler<DeletePolicyAcknowledgmentCommand, ErrorOr<GenericResponse>>
{
    private readonly ApplicationDbContext _context;

    public DeletePolicyAcknowledgmentCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse>> Handle(
        DeletePolicyAcknowledgmentCommand request,
        CancellationToken cancellationToken)
    {
        var acknowledgment = await _context.PolicyAcknowledgments
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (acknowledgment == null)
        {
            return Error.NotFound(description: "Policy acknowledgment not found");
        }

        _context.PolicyAcknowledgments.Remove(acknowledgment);
        await _context.SaveChangesAsync(cancellationToken);

        return new GenericResponse
        {
            Success = true,
            Message = "Policy acknowledgment deleted successfully"
        };
    }
}
