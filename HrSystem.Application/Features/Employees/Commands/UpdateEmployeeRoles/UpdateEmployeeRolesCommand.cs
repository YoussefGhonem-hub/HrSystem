using ErrorOr;
using HrSystem.Domain.Entities.Account;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Employees.Commands.UpdateEmployeeRoles;

public record UpdateEmployeeRolesCommand(Guid EmployeeId,List<Guid> RoleIds) : IRequest<ErrorOr<GenericResponse<bool>>>;

public class UpdateEmployeeRolesCommandHandler : IRequestHandler<UpdateEmployeeRolesCommand, ErrorOr<GenericResponse<bool>>>
{
    private readonly ApplicationDbContext _context;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public UpdateEmployeeRolesCommandHandler(ApplicationDbContext context, RoleManager<ApplicationRole> roleManager)
    {
        _context = context;
        _roleManager = roleManager;
    }

    public async Task<ErrorOr<GenericResponse<bool>>> Handle(UpdateEmployeeRolesCommand request, CancellationToken cancellationToken)
    {
        var orgId = CurrentUser.OrganizationId;
        if (!orgId.HasValue)
        {
            return Error.Unauthorized(description: "No organization context");
        }

        var requesterBranchId = CurrentUser.BranchId;

        var normalizedRoles = CurrentUser.Roles
            .Select(RoleNames.Normalize)
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .ToArray();

        // Authorization: HR/Admin only
        var isHr = normalizedRoles.Any(r => string.Equals(r, RoleNames.HRManager, StringComparison.OrdinalIgnoreCase)) ||
                   normalizedRoles.Any(r => string.Equals(r, RoleNames.HRSpecialist, StringComparison.OrdinalIgnoreCase)) ||
                   normalizedRoles.Any(r => string.Equals(r, RoleNames.OrganizationAdmin, StringComparison.OrdinalIgnoreCase)) ||
                   normalizedRoles.Any(r => string.Equals(r, RoleNames.SuperAdmin, StringComparison.OrdinalIgnoreCase));

        if (!isHr)
        {
            return Error.Forbidden(description: "Not authorized to update employee roles");
        }

        // Validate employee and linked user
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId && e.TenantId == orgId.Value, cancellationToken);

        if (employee == null)
        {
            return Error.NotFound(code: "Employee.NotFound", description: "Employee not found");
        }

        var editAccess = await EmployeeEditAuthorizationGuard.EnsureCanEditAsync(
            _context,
            employee.Id,
            employee.UserId,
            cancellationToken);
        if (editAccess.IsError)
        {
            return editAccess.Errors;
        }

        if (!employee.UserId.HasValue)
        {
            return Error.Validation(code: "Employee.UserMissing", description: "Employee is not linked to a user account");
        }

        var branchId = (requesterBranchId.HasValue && requesterBranchId.Value != Guid.Empty)
            ? requesterBranchId.Value
            : employee.BranchId;

        if (!branchId.HasValue || branchId.Value == Guid.Empty)
        {
            return Error.Validation(code: "Branch.Required", description: "Target employee must be linked to a branch to update branch-scoped roles");
        }

        // Validate branch
        var branch = await _context.Branches
            .FirstOrDefaultAsync(b => b.Id == branchId.Value && b.OrganizationId == orgId.Value, cancellationToken);

        if (branch == null)
        {
            return Error.NotFound(code: "Branch.NotFound", description: "Branch not found");
        }

        // Validate roles and resolve names
        var roleNames = new List<string>();
        foreach (var roleId in request.RoleIds)
        {
            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            if (role == null)
            {
                return Error.NotFound(code: "Role.NotFound", description: $"Role with ID {roleId} not found");
            }
            roleNames.Add(role.Name ?? role.DisplayName ?? string.Empty);
        }

        // Remove existing branch-scoped roles for this user/branch
        var existingRoles = await _context.UserBranchRoles
            .Where(ubr => ubr.UserId == employee.UserId.Value && ubr.BranchId == branchId.Value)
            .ToListAsync(cancellationToken);

        _context.UserBranchRoles.RemoveRange(existingRoles);

        // Add new roles
        foreach (var roleName in roleNames)
        {
            var userBranchRole = new UserBranchRole
            {
                Id = Guid.NewGuid(),
                UserId = employee.UserId.Value,
                BranchId = branchId.Value,
                RoleName = roleName
            };

            await _context.UserBranchRoles.AddAsync(userBranchRole, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new GenericResponse<bool>
        {
            Success = true,
            Message = $"Employee roles updated successfully for branch {branch.NameEn}",
            Data = true
        };
    }
}
