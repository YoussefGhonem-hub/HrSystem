using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Users.Commands.DeactivateAccount;

public record DeactivateAccountCommand(Guid UserId) : IRequest<ErrorOr<GenericResponse<bool>>>;

public class DeactivateAccountCommandHandler : IRequestHandler<DeactivateAccountCommand, ErrorOr<GenericResponse<bool>>>
{
    private readonly ApplicationDbContext _context;

    public DeactivateAccountCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<bool>>> Handle(DeactivateAccountCommand request, CancellationToken cancellationToken)
    {
        var orgId = CurrentUser.OrganizationId;
        if (!orgId.HasValue)
        {
            return Error.Unauthorized(description: "No organization context");
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.OrganizationId == orgId.Value && u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            return Error.NotFound(code: "User.NotFound", description: "User not found");
        }

        if (!user.IsActive)
        {
            return Error.Conflict(code: "User.AlreadyDeactivated", description: "User account is already deactivated");
        }

        user.IsActive = false;
        user.ModifiedDate = DateTimeOffset.UtcNow;
        user.ModifiedBy = CurrentUser.Id;

        await _context.SaveChangesAsync(cancellationToken);

        return new GenericResponse<bool>
        {
            Success = true,
            Message = "User account deactivated successfully",
            Data = true
        };
    }
}
