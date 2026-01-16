using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Branches.Commands.DeleteBranch;

public class DeleteBranchCommandHandler : IRequestHandler<DeleteBranchCommand, ErrorOr<GenericResponse>>
{
    private readonly ApplicationDbContext _context;

    public DeleteBranchCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse>> Handle(
        DeleteBranchCommand request,
        CancellationToken cancellationToken)
    {
        var branch = await _context.Branches
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

        if (branch == null)
        {
            return Error.NotFound(description: "Branch not found");
        }

        // Soft delete by setting IsActive to false
        branch.IsActive = false;
        branch.ClosingDate = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new GenericResponse
        {
            Success = true,
            Message = "Branch deleted successfully"
        };
    }
}
