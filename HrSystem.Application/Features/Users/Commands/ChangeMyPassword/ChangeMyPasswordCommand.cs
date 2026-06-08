using ErrorOr;
using HrSystem.Domain.Entities.Account;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Users.Commands.ChangeMyPassword;

public record ChangeMyPasswordCommand(
    string CurrentPassword,
    string NewPassword
) : IRequest<ErrorOr<GenericResponse<bool>>>;

public class ChangeMyPasswordCommandHandler : IRequestHandler<ChangeMyPasswordCommand, ErrorOr<GenericResponse<bool>>>
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ChangeMyPasswordCommandHandler(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<ErrorOr<GenericResponse<bool>>> Handle(
        ChangeMyPasswordCommand request,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUser.Id;
        if (!userId.HasValue)
            return Error.Unauthorized(description: "User not authenticated.");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId.Value, cancellationToken);
        if (user == null)
            return Error.NotFound(description: "User not found.");

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Error.Failure(description: $"Password change failed: {errors}");
        }

        user.ModifiedDate = DateTimeOffset.UtcNow;
        user.ModifiedBy = userId;
        await _context.SaveChangesAsync(cancellationToken);

        return GenericResponse<bool>.SuccessResult(true, "Password changed successfully.");
    }
}
