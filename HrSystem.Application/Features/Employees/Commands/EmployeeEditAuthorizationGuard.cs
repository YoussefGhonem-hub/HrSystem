using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Employees.Commands;

internal static class EmployeeEditAuthorizationGuard
{
    public static async Task<ErrorOr<Success>> EnsureCanEditAsync(
        ApplicationDbContext context,
        Guid targetEmployeeId,
        Guid? targetUserId,
        CancellationToken cancellationToken)
    {
        var isHrManager = CurrentUser.Roles.Any(r =>
            r.Equals(RoleNames.HRManager, StringComparison.OrdinalIgnoreCase));

        if (isHrManager && CurrentUser.EmployeeId.HasValue && CurrentUser.EmployeeId.Value == targetEmployeeId)
        {
            return Error.Forbidden("Employee.SelfEditForbidden", "HR Manager cannot edit their own profile.");
        }

        if (!CurrentUser.IsSuperAdmin && await IsAdminProfileAsync(context, targetUserId, cancellationToken))
        {
            return Error.Forbidden("Employee.AdminProfileEditForbidden", "You are not allowed to edit admin profiles.");
        }

        return Result.Success;
    }

    private static async Task<bool> IsAdminProfileAsync(
        ApplicationDbContext context,
        Guid? userId,
        CancellationToken cancellationToken)
    {
        if (!userId.HasValue)
        {
            return false;
        }

        var roleNames = await context.UserRoles
            .Where(ur => ur.UserId == userId.Value)
            .Join(context.Roles,
                ur => ur.RoleId,
                role => role.Id,
                (_, role) => role.Name)
            .ToListAsync(cancellationToken);

        return roleNames.Any(name =>
            string.Equals(name, RoleNames.OrganizationAdmin, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(name, RoleNames.SuperAdmin, StringComparison.OrdinalIgnoreCase));
    }
}