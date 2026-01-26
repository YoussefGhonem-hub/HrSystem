using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Leave.Commands.DeleteLeavePolicy;

public record DeleteLeavePolicyCommand(Guid Id) : IRequest<ErrorOr<GenericResponse<bool>>>;

public class DeleteLeavePolicyCommandHandler : IRequestHandler<DeleteLeavePolicyCommand, ErrorOr<GenericResponse<bool>>>
{
    private readonly ApplicationDbContext _context;

    public DeleteLeavePolicyCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<bool>>> Handle(DeleteLeavePolicyCommand request, CancellationToken cancellationToken)
    {
        var orgId = CurrentUser.OrganizationId;
        if (!orgId.HasValue)
        {
            return Error.Unauthorized(description: "No organization context");
        }

        var policy = await _context.Set<HrSystem.Domain.Entities.Leave.LeavePolicy>()
            .FirstOrDefaultAsync(lp => lp.Id == request.Id && lp.TenantId == orgId.Value, cancellationToken);

        if (policy == null)
        {
            return Error.NotFound(code: "LeavePolicy.NotFound", description: "Leave policy not found");
        }

        // Check if policy is being used
        var hasBalances = await _context.Set<HrSystem.Domain.Entities.Leave.LeaveBalance>()
            .AnyAsync(lb => lb.LeavePolicyId == request.Id, cancellationToken);

        if (hasBalances)
        {
            return Error.Conflict(code: "LeavePolicy.InUse", description: "Cannot delete leave policy that has associated leave balances");
        }

        policy.MarkAsDeleted(CurrentUser.Id ?? Guid.Empty);
        await _context.SaveChangesAsync(cancellationToken);

        return new GenericResponse<bool>
        {
            Success = true,
            Message = "Leave policy deleted successfully",
            Data = true
        };
    }
}
