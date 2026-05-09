using ErrorOr;
using HrSystem.Application.Features.Employees.Commands.CreateEmployee;
using HrSystem.Application.Features.Employees.Queries.GetEmployeeById;
using HrSystem.Domain.Entities.Account;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Employees.Commands.UpdateEmployeeJobInfo;

public class UpdateEmployeeJobInfoCommandHandler : IRequestHandler<UpdateEmployeeJobInfoCommand, ErrorOr<GenericResponse<EmployeeDto>>>
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public UpdateEmployeeJobInfoCommandHandler(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<ErrorOr<GenericResponse<EmployeeDto>>> Handle(
        UpdateEmployeeJobInfoCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        if (employee is null)
        {
            return Error.NotFound(description: "Employee not found");
        }

        var isHrManager = CurrentUser.Roles.Any(r =>
            r.Equals(RoleNames.HRManager, StringComparison.OrdinalIgnoreCase));
        var isSuperAdmin = CurrentUser.IsSuperAdmin;

        if (isHrManager && CurrentUser.EmployeeId.HasValue && CurrentUser.EmployeeId.Value == employee.Id)
        {
            return Error.Forbidden("Employee.SelfEditForbidden", "HR Manager cannot edit their own profile.");
        }

        if (!isSuperAdmin && await IsAdminProfileAsync(employee.UserId, cancellationToken))
        {
            return Error.Forbidden("Employee.AdminProfileEditForbidden", "You are not allowed to edit admin profiles.");
        }

        employee.DepartmentId = request.DepartmentId;
        employee.JobTitleId = request.JobTitleId;
        employee.DirectManagerId = request.DirectManagerId;
        employee.BranchId = request.BranchId;
        employee.ContractTypeId = request.ContractTypeId;
        employee.HiringDate = request.HiringDate;
        employee.ProbationPeriodMonths = request.ProbationPeriodMonths;
        employee.ProbationEndDate = request.HiringDate.AddMonths(request.ProbationPeriodMonths);
        employee.StatusId = EmployeeStatusIds.Active;

        await _context.SaveChangesAsync(cancellationToken);

        // Handle role change if RoleId is provided
        if (request.RoleId.HasValue && request.RoleId.Value != Guid.Empty)
        {
            var role = await _roleManager.FindByIdAsync(request.RoleId.Value.ToString());
            if (role == null)
            {
                return Error.NotFound("Role.NotFound", "The specified role was not found");
            }

            if (employee.UserId.HasValue)
            {
                var user = await _userManager.FindByIdAsync(employee.UserId.Value.ToString());
                if (user != null)
                {
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    if (currentRoles.Count > 0)
                        await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    await _userManager.AddToRoleAsync(user, role.Name!);

                    var branchId = employee.BranchId ?? request.BranchId;
                    if (branchId.HasValue)
                    {
                        var existingBranchRoles = await _context.UserBranchRoles
                            .Where(r => r.UserId == user.Id && r.BranchId == branchId.Value)
                            .ToListAsync(cancellationToken);
                        _context.UserBranchRoles.RemoveRange(existingBranchRoles);
                        _context.UserBranchRoles.Add(new UserBranchRole
                        {
                            UserId = user.Id,
                            BranchId = branchId.Value,
                            RoleName = role.Name!
                        });
                        await _context.SaveChangesAsync(cancellationToken);
                    }
                }
            }
            else
            {
                // Employee has no user account — create one
                var existingUser = await _userManager.FindByEmailAsync(employee.Email);
                if (existingUser != null)
                {
                    return Error.Conflict("User.EmailExists", $"A user with email '{employee.Email}' already exists");
                }

                var emailPrefix = employee.Email.Split('@')[0];
                var defaultPassword = $"Hr@{employee.NationalId[..Math.Min(6, employee.NationalId.Length)]}Xx1";

                var user = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = employee.Email,
                    Email = employee.Email,
                    EmailConfirmed = true,
                    FullName = $"{employee.FirstNameEn} {employee.LastNameEn}".Trim(),
                    IsActive = true,
                    OrganizationId = employee.TenantId,
                    BranchId = employee.BranchId ?? request.BranchId,
                    EmployeeId = employee.Id,
                };

                var createResult = await _userManager.CreateAsync(user, defaultPassword);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                    return Error.Failure("User.CreateFailed", errors);
                }

                await _userManager.AddToRoleAsync(user, role.Name!);
                employee.UserId = user.Id;

                if (employee.BranchId.HasValue)
                {
                    _context.UserBranchRoles.Add(new UserBranchRole
                    {
                        UserId = user.Id,
                        BranchId = employee.BranchId.Value,
                        RoleName = role.Name!
                    });
                }

                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        var dto = await EmployeeCommandHelper.BuildEmployeeDtoAsync(_context, employee.Id, cancellationToken);

        return new GenericResponse<EmployeeDto>
        {
            Success = true,
            Message = "Employee job info updated successfully",
            Data = dto
        };
    }

    private async Task<bool> IsAdminProfileAsync(Guid? userId, CancellationToken cancellationToken)
    {
        if (!userId.HasValue)
            return false;

        var roleNames = await _context.UserRoles
            .Where(ur => ur.UserId == userId.Value)
            .Join(_context.Roles,
                ur => ur.RoleId,
                role => role.Id,
                (_, role) => role.Name)
            .ToListAsync(cancellationToken);

        return roleNames.Any(name =>
            string.Equals(name, RoleNames.OrganizationAdmin, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(name, RoleNames.SuperAdmin, StringComparison.OrdinalIgnoreCase));
    }
}
