using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Branches.Queries.GetBranchById;

public class GetBranchByIdQueryHandler : IRequestHandler<GetBranchByIdQuery, ErrorOr<GenericResponse<BranchDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetBranchByIdQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<BranchDto>>> Handle(
        GetBranchByIdQuery request,
        CancellationToken cancellationToken)
    {
        var branch = await _context.Branches
            .Include(b => b.BranchManager)
            .Include(b => b.Employees)
            .Include(b => b.Departments)
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

        if (branch == null)
        {
            return Error.NotFound(description: "Branch not found");
        }

        var dto = branch.Adapt<BranchDto>();

        return new GenericResponse<BranchDto>
        {
            Success = true,
            Data = dto
        };
    }
}
