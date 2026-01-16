using ErrorOr;
using HrSystem.Application.Features.Branches.Queries.GetBranchById;
using HrSystem.Domain.Entities.Organization;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Branches.Commands.CreateBranch;

public record CreateBranchCommand(CreateBranchDto Branch) : IRequest<ErrorOr<GenericResponse<BranchDto>>>;

public class CreateBranchCommandHandler : IRequestHandler<CreateBranchCommand, ErrorOr<GenericResponse<BranchDto>>>
{
    private readonly ApplicationDbContext _context;

    public CreateBranchCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<BranchDto>>> Handle(
        CreateBranchCommand request,
        CancellationToken cancellationToken)
    {
        var branch = request.Branch.Adapt<Branch>();
        branch.OrganizationId = Guid.NewGuid(); // Should come from CurrentUser.OrganizationId
        branch.TenantId = Guid.NewGuid();

        _context.Branches.Add(branch);
        await _context.SaveChangesAsync(cancellationToken);

        var createdBranch = await _context.Branches
            .Include(b => b.BranchManager)
            .Include(b => b.Employees)
            .Include(b => b.Departments)
            .FirstAsync(b => b.Id == branch.Id, cancellationToken);

        var dto = createdBranch.Adapt<BranchDto>();

        return new GenericResponse<BranchDto>
        {
            Success = true,
            Message = "Branch created successfully",
            Data = dto
        };
    }
}
