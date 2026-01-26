using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Users.Commands.UpdateUser;

public record UpdateUserCommand(
    Guid Id,
    string? FullName,
    string? UserName,
    bool? IsActive
) : IRequest<ErrorOr<GenericResponse<bool>>>;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, ErrorOr<GenericResponse<bool>>>
{
    private readonly ApplicationDbContext _context;

    public UpdateUserCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<bool>>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var orgId = CurrentUser.OrganizationId;
        if (!orgId.HasValue)
        {
            return Error.Unauthorized(description: "No organization context");
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.OrganizationId == orgId.Value && u.Id == request.Id, cancellationToken);

        if (user == null)
        {
            return Error.NotFound(code: "User.NotFound", description: "User not found");
        }

        if (request.FullName is not null)
            user.FullName = request.FullName;

        if (request.UserName is not null)
            user.UserName = request.UserName;

        if (request.IsActive.HasValue)
            user.IsActive = request.IsActive.Value;

        user.ModifiedDate = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new GenericResponse<bool>
        {
            Success = true,
            Message = "User updated",
            Data = true
        };
    }
}
