using ErrorOr;
using HrSystem.Application.Features.Users.Queries.GetAccountSettings;
using HrSystem.Domain.Entities.Account;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Users.Commands.UpdateAccountSettings;

/// <summary>
/// Command to update account settings (username, email, status)
/// </summary>
public record UpdateAccountSettingsCommand : IRequest<ErrorOr<GenericResponse<AccountSettingsDto>>>
{
    /// <summary>
    /// User ID to update
    /// </summary>
    public Guid UserId { get; init; }
    
    /// <summary>
    /// New username
    /// </summary>
    public string? UserName { get; init; }
    
    /// <summary>
    /// Account status: "Active" or "Inactive"
    /// </summary>
    public string? AccountStatus { get; init; }
    
    /// <summary>
    /// Work email (used for login)
    /// </summary>
    public string? WorkEmail { get; init; }
}

public class UpdateAccountSettingsCommandHandler : IRequestHandler<UpdateAccountSettingsCommand, ErrorOr<GenericResponse<AccountSettingsDto>>>
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public UpdateAccountSettingsCommandHandler(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<ErrorOr<GenericResponse<AccountSettingsDto>>> Handle(
        UpdateAccountSettingsCommand request, 
        CancellationToken cancellationToken)
    {
        var orgId = CurrentUser.OrganizationId;
        if (!orgId.HasValue)
        {
            return Error.Unauthorized(description: "No organization context");
        }

        // Authorization: Only HR/Admin or self can update
        var isHr = CurrentUser.Roles?.Contains(RoleNames.HRManager) == true ||
                   CurrentUser.Roles?.Contains(RoleNames.HRSpecialist) == true ||
                   CurrentUser.Roles?.Contains(RoleNames.OrganizationAdmin) == true ||
                   CurrentUser.Roles?.Contains(RoleNames.SuperAdmin) == true;

        var isSelf = CurrentUser.Id == request.UserId;

        if (!isHr && !isSelf)
        {
            return Error.Forbidden(description: "Not authorized to update this account");
        }

        var user = await _context.Users
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.OrganizationId == orgId.Value && u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            return Error.NotFound(code: "User.NotFound", description: "User not found");
        }

        // Update username if provided
        if (!string.IsNullOrWhiteSpace(request.UserName) && request.UserName != user.UserName)
        {
            // Check if username is already taken
            var existingUser = await _userManager.FindByNameAsync(request.UserName);
            if (existingUser != null && existingUser.Id != user.Id)
            {
                return Error.Conflict(code: "User.UserNameTaken", description: "Username is already taken");
            }
            user.UserName = request.UserName;
            user.NormalizedUserName = request.UserName.ToUpperInvariant();
        }

        // Update email if provided
        if (!string.IsNullOrWhiteSpace(request.WorkEmail) && request.WorkEmail != user.Email)
        {
            // Check if email is already taken
            var existingEmail = await _userManager.FindByEmailAsync(request.WorkEmail);
            if (existingEmail != null && existingEmail.Id != user.Id)
            {
                return Error.Conflict(code: "User.EmailTaken", description: "Email is already taken");
            }
            user.Email = request.WorkEmail;
            user.NormalizedEmail = request.WorkEmail.ToUpperInvariant();
        }

        // Update account status if provided (only HR/Admin can change status)
        if (!string.IsNullOrWhiteSpace(request.AccountStatus) && isHr)
        {
            user.IsActive = request.AccountStatus.Equals("Active", StringComparison.OrdinalIgnoreCase);
        }

        user.ModifiedDate = DateTimeOffset.UtcNow;
        user.ModifiedBy = CurrentUser.Id;

        await _context.SaveChangesAsync(cancellationToken);

        // Return updated settings
        var result = new AccountSettingsDto
        {
            Id = user.Id,
            UserName = user.UserName,
            AccountStatus = user.IsActive ? "Active" : "Inactive",
            IsActive = user.IsActive,
            WorkEmail = user.Email,
            LastLogin = user.LastLogin,
            FullName = user.FullName,
            EmployeeId = user.EmployeeId,
            EmployeeName = user.Employee?.FullNameEn
        };

        return new GenericResponse<AccountSettingsDto>
        {
            Success = true,
            Message = "Account settings updated successfully",
            Data = result
        };
    }
}
