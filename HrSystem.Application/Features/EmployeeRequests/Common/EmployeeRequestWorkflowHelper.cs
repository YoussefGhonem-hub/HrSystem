using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Common;

internal static class EmployeeRequestWorkflowHelper
{
    private static readonly string[] HrRoleNames =
    {
        RoleNames.HRManager,
        RoleNames.HRSpecialist,
        RoleNames.OrganizationAdmin
    };

    public static async Task<EmployeeRequestStatus> ResolveInitialStatusAsync(
        ApplicationDbContext context,
        Guid? directManagerId,
        bool requiresManagerApproval,
        CancellationToken cancellationToken)
    {
        if (!requiresManagerApproval)
            return EmployeeRequestStatus.Pending;

        var managerIsHrApprover = await IsDirectManagerHrApproverAsync(context, directManagerId, cancellationToken);
        return managerIsHrApprover
            ? EmployeeRequestStatus.ManagerApproved
            : EmployeeRequestStatus.Pending;
    }

    public static async Task<bool> IsDirectManagerHrApproverAsync(
        ApplicationDbContext context,
        Guid? directManagerId,
        CancellationToken cancellationToken)
    {
        if (!directManagerId.HasValue)
            return false;

        var manager = await context.Employees
            .AsNoTracking()
            .Where(e => e.Id == directManagerId.Value)
            .Select(e => new
            {
                e.UserId,
                DepartmentCode = e.Department != null ? e.Department.Code : null,
                DepartmentNameEn = e.Department != null ? e.Department.NameEn : null,
                DepartmentNameAr = e.Department != null ? e.Department.NameAr : null
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (manager is null)
            return false;

        if (manager.UserId.HasValue)
        {
            // Check Identity roles (AspNetUserRoles) — source of truth after CreateUserWithBranchRoles.
            var identityRoleNames = await context.UserRoles
                .Join(
                    context.Roles,
                    userRole => userRole.RoleId,
                    role => role.Id,
                    (userRole, role) => new { userRole.UserId, role.Name })
                .Where(x => x.UserId == manager.UserId.Value && x.Name != null)
                .Select(x => x.Name!)
                .ToListAsync(cancellationToken);

            // Also check UserBranchRoles — updated by ChangeUserRoleCommand.
            var branchRoleNames = await context.UserBranchRoles
                .IgnoreQueryFilters()
                .Where(ubr => ubr.UserId == manager.UserId.Value && ubr.RoleName != null)
                .Select(ubr => ubr.RoleName!)
                .ToListAsync(cancellationToken);

            var hasHrRole = identityRoleNames
                .Concat(branchRoleNames)
                .Any(name => HrRoleNames.Contains(RoleNames.Normalize(name), StringComparer.OrdinalIgnoreCase));

            if (hasHrRole)
                return true;
        }

        return LooksLikeHrDepartment(manager.DepartmentCode, manager.DepartmentNameEn, manager.DepartmentNameAr);
    }

    private static bool LooksLikeHrDepartment(string? departmentCode, string? departmentNameEn, string? departmentNameAr)
    {
        return ContainsHrToken(departmentCode)
               || ContainsHrToken(departmentNameEn)
               || ContainsHrToken(departmentNameAr);
    }

    private static bool ContainsHrToken(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var normalized = value.Trim().ToUpperInvariant();
        return normalized.Contains("HR")
               || normalized.Contains("HUMAN RESOURCE")
               || normalized.Contains("HUMAN RESOURCES")
               || normalized.Contains("PEOPLE");
    }
}
