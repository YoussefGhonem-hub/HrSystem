using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Users.Commands.ActivateAccount;

public record ActivateAccountCommand(Guid UserId) : IRequest<ErrorOr<GenericResponse<bool>>>;

public class ActivateAccountCommandHandler : IRequestHandler<ActivateAccountCommand, ErrorOr<GenericResponse<bool>>>
{
    private readonly ApplicationDbContext _context;

    public ActivateAccountCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<bool>>> Handle(ActivateAccountCommand request, CancellationToken cancellationToken)
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

        if (user.IsActive)
        {
            return Error.Conflict(code: "User.AlreadyActive", description: "User account is already active");
        }

        user.IsActive = true;
        user.ModifiedDate = DateTimeOffset.UtcNow;
        user.ModifiedBy = CurrentUser.Id;

        await _context.SaveChangesAsync(cancellationToken);

        return new GenericResponse<bool>
        {
            Success = true,
            Message = "User account activated successfully",
            Data = true
        };
    }
}
