using ErrorOr;
using HrSystem.Application.Features.Branches.Queries.GetBranchById;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Branches.Commands.UpdateBranch;

public class UpdateBranchCommandHandler : IRequestHandler<UpdateBranchCommand, ErrorOr<GenericResponse<BranchDto>>>
{
    private readonly ApplicationDbContext _context;

    public UpdateBranchCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<BranchDto>>> Handle(
        UpdateBranchCommand request,
        CancellationToken cancellationToken)
    {
        var branch = await _context.Branches
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

        if (branch == null)
        {
            return Error.NotFound(description: "Branch not found");
        }

        request.Branch.Adapt(branch);

        await _context.SaveChangesAsync(cancellationToken);

        var updatedBranch = await _context.Branches
            .Include(b => b.BranchManager)
            .Include(b => b.Employees)
            .Include(b => b.Departments)
            .FirstAsync(b => b.Id == branch.Id, cancellationToken);

        var dto = updatedBranch.Adapt<BranchDto>();

        return new GenericResponse<BranchDto>
        {
            Success = true,
            Message = "Branch updated successfully",
            Data = dto
        };
    }
}
