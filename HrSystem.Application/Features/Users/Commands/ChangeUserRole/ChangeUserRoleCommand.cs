using ErrorOr;
using HrSystem.Domain.Entities.Account;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Users.Commands.ChangeUserRole;

public record ChangeUserRoleCommand(
    Guid UserId,
    Guid BranchId,
    List<Guid> RoleIds
) : IRequest<ErrorOr<GenericResponse<bool>>>;

public class ChangeUserRoleCommandHandler : IRequestHandler<ChangeUserRoleCommand, ErrorOr<GenericResponse<bool>>>
{
    private readonly ApplicationDbContext _context;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public ChangeUserRoleCommandHandler(
        ApplicationDbContext context,
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task<ErrorOr<GenericResponse<bool>>> Handle(ChangeUserRoleCommand request, CancellationToken cancellationToken)
    {
        var orgId = CurrentUser.OrganizationId;
        if (!orgId.HasValue)
        {
            return Error.Unauthorized(description: "No organization context");
        }

        // Validate user
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId && u.OrganizationId == orgId.Value, cancellationToken);

        if (user == null)
        {
            return Error.NotFound(code: "User.NotFound", description: "User not found");
        }

        // Validate branch
        var branch = await _context.Branches
            .FirstOrDefaultAsync(b => b.Id == request.BranchId && b.OrganizationId == orgId.Value, cancellationToken);

        if (branch == null)
        {
            return Error.NotFound(code: "Branch.NotFound", description: "Branch not found");
        }

        // Validate and resolve role names from role IDs
        var roleNames = new List<string>();
        foreach (var roleId in request.RoleIds)
        {
            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            if (role == null)
            {
                return Error.NotFound(code: "Role.NotFound", description: $"Role with ID {roleId} not found");
            }
            roleNames.Add(role.Name!);
        }

        // Remove existing roles for this user and branch
        var existingRoles = await _context.UserBranchRoles
            .Where(ubr => ubr.UserId == request.UserId && ubr.BranchId == request.BranchId)
            .ToListAsync(cancellationToken);

        _context.UserBranchRoles.RemoveRange(existingRoles);

        // Add new roles
        foreach (var roleName in roleNames)
        {
            var userBranchRole = new UserBranchRole
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                BranchId = request.BranchId,
                RoleName = roleName
            };

            await _context.UserBranchRoles.AddAsync(userBranchRole, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Sync Identity roles so that GetRolesAsync (used in login/JWT generation) reflects
        // the latest role assignments stored in UserBranchRoles.
        var allBranchRoleNames = await _context.UserBranchRoles
            .IgnoreQueryFilters()
            .Where(ubr => ubr.UserId == request.UserId && ubr.RoleName != null)
            .Select(ubr => ubr.RoleName!)
            .Distinct()
            .ToListAsync(cancellationToken);

        var currentIdentityRoles = await _userManager.GetRolesAsync(user);
        var toRemove = currentIdentityRoles.Except(allBranchRoleNames, StringComparer.OrdinalIgnoreCase).ToList();
        var toAdd = allBranchRoleNames.Except(currentIdentityRoles, StringComparer.OrdinalIgnoreCase).ToList();

        if (toRemove.Count > 0)
            await _userManager.RemoveFromRolesAsync(user, toRemove);
        if (toAdd.Count > 0)
            await _userManager.AddToRolesAsync(user, toAdd);

        return new GenericResponse<bool>
        {
            Success = true,
            Message = $"User roles updated successfully for branch {branch.NameEn}",
            Data = true
        };
    }
}
