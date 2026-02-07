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

        var branchId = CurrentUser.BranchId ;
        if (branchId == Guid.Empty)
        {
            return Error.Validation(code: "Branch.Required", description: "Branch context is required to update roles");
        }

        // Authorization: HR/Admin only
        var isHr = CurrentUser.Roles?.Contains(RoleNames.HRManager) == true ||
                   CurrentUser.Roles?.Contains(RoleNames.HRSpecialist) == true ||
                   CurrentUser.Roles?.Contains(RoleNames.OrganizationAdmin) == true ||
                   CurrentUser.Roles?.Contains(RoleNames.SuperAdmin) == true;

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

        if (!employee.UserId.HasValue)
        {
            return Error.Validation(code: "Employee.UserMissing", description: "Employee is not linked to a user account");
        }

        // Validate branch
        var branch = await _context.Branches
            .FirstOrDefaultAsync(b => b.Id == branchId && b.OrganizationId == orgId.Value, cancellationToken);

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
            .Where(ubr => ubr.UserId == employee.UserId.Value && ubr.BranchId == branchId)
            .ToListAsync(cancellationToken);

        _context.UserBranchRoles.RemoveRange(existingRoles);

        // Add new roles
        foreach (var roleName in roleNames)
        {
            var userBranchRole = new UserBranchRole
            {
                Id = Guid.NewGuid(),
                UserId = employee.UserId.Value,
                BranchId = branchId,
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
