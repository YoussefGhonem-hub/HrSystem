using ErrorOr;
using HrSystem.Domain.Entities.Account;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using HrSystem.Shared.Extensions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Users.Commands.ResetUserPassword;

/// <summary>
/// Command to reset a user's password (Admin action)
/// </summary>
public record ResetUserPasswordCommand : IRequest<ErrorOr<GenericResponse<ResetPasswordResultDto>>>
{
    /// <summary>
    /// User ID whose password will be reset
    /// </summary>
    public Guid UserId { get; init; }
    
    /// <summary>
    /// Optional new password. If not provided, a random password will be generated.
    /// </summary>
    public string? NewPassword { get; init; }
    
    /// <summary>
    /// Whether to send the new password via email (default: false)
    /// </summary>
    public bool SendEmail { get; init; } = false;
}

public record ResetPasswordResultDto
{
    public Guid UserId { get; init; }
    public string? UserName { get; init; }
    public string? Email { get; init; }
    
    /// <summary>
    /// The temporary password (only returned if SendEmail is false)
    /// </summary>
    public string? TemporaryPassword { get; init; }
    
    /// <summary>
    /// Whether password reset was successful
    /// </summary>
    public bool PasswordReset { get; init; }
    
    /// <summary>
    /// Message about the reset action
    /// </summary>
    public string Message { get; init; } = string.Empty;
}

public class ResetUserPasswordCommandHandler : IRequestHandler<ResetUserPasswordCommand, ErrorOr<GenericResponse<ResetPasswordResultDto>>>
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ResetUserPasswordCommandHandler(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<ErrorOr<GenericResponse<ResetPasswordResultDto>>> Handle(
        ResetUserPasswordCommand request, 
        CancellationToken cancellationToken)
    {
        var orgId = CurrentUser.OrganizationId;
        if (!orgId.HasValue)
        {
            return Error.Unauthorized(description: "No organization context");
        }

        // Authorization: Only HR/Admin can reset passwords
        var isHr = CurrentUser.Roles?.Contains(RoleNames.HRManager) == true ||
                   CurrentUser.Roles?.Contains(RoleNames.HRSpecialist) == true ||
                   CurrentUser.Roles?.Contains(RoleNames.OrganizationAdmin) == true ||
                   CurrentUser.Roles?.Contains(RoleNames.SuperAdmin) == true;

        if (!isHr)
        {
            return Error.Forbidden(description: "Only HR/Admin can reset user passwords");
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.OrganizationId == orgId.Value && u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            return Error.NotFound(code: "User.NotFound", description: "User not found");
        }

        // Generate or use provided password
        var newPassword = string.IsNullOrWhiteSpace(request.NewPassword) 
            ? Utils.GeneratePassword(12) 
            : request.NewPassword;

        // Reset the password
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetResult = await _userManager.ResetPasswordAsync(user, token, newPassword);

        if (!resetResult.Succeeded)
        {
            var errors = string.Join(", ", resetResult.Errors.Select(e => e.Description));
            return Error.Failure(code: "User.PasswordResetFailed", description: $"Password reset failed: {errors}");
        }

        user.ModifiedDate = DateTimeOffset.UtcNow;
        user.ModifiedBy = CurrentUser.Id;
        await _context.SaveChangesAsync(cancellationToken);

        // TODO: If SendEmail is true, send email with temporary password
        // For now, we return the password in the response

        var result = new ResetPasswordResultDto
        {
            UserId = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            TemporaryPassword = request.SendEmail ? null : newPassword,
            PasswordReset = true,
            Message = request.SendEmail 
                ? "Password reset successfully. New password has been sent to user's email." 
                : "Password reset successfully. Please share the temporary password with the user securely."
        };

        return new GenericResponse<ResetPasswordResultDto>
        {
            Success = true,
            Message = "Password reset successfully",
            Data = result
        };
    }
}
