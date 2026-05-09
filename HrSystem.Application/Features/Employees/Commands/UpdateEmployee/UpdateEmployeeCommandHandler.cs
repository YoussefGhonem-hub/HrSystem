using ErrorOr;
using HrSystem.Application.Features.Employees.Queries.GetEmployeeById;
using HrSystem.Domain.Entities.Account;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Employees.Commands.UpdateEmployee;

public class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand, ErrorOr<GenericResponse<EmployeeDto>>>
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public UpdateEmployeeCommandHandler(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<ErrorOr<GenericResponse<EmployeeDto>>> Handle(
        UpdateEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (employee == null)
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

        employee.FirstNameAr = request.FirstNameAr;
        employee.LastNameAr = request.LastNameAr;
        employee.FirstNameEn = request.FirstNameEn;
        employee.LastNameEn = request.LastNameEn;
        employee.PassportNumber = request.PassportNumber;
        employee.MaritalStatusId = request.MaritalStatusId;
        employee.Email = request.Email;
        employee.PhoneNumber = request.PhoneNumber;
        employee.MobileNumber = request.MobileNumber;
        employee.AddressAr = request.AddressAr;
        employee.AddressEn = request.AddressEn;
        employee.City = request.City;
        employee.Country = request.Country;
        employee.DepartmentId = request.DepartmentId;
        employee.JobTitleId = request.JobTitleId;
        employee.DirectManagerId = request.DirectManagerId;
        employee.BranchId = request.BranchId;
        employee.ContractTypeId = request.ContractTypeId;
        employee.StatusId = request.StatusId;

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
                // Employee already has a user — update their role
                var user = await _userManager.FindByIdAsync(employee.UserId.Value.ToString());
                if (user != null)
                {
                    // Remove all current roles and assign new one
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    if (currentRoles.Count > 0)
                        await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    await _userManager.AddToRoleAsync(user, role.Name!);

                    // Update UserBranchRoles
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
                    CreatedDate = DateTimeOffset.UtcNow
                };

                var defaultPassword = $"Hr@{employee.NationalId[..Math.Min(6, employee.NationalId.Length)]}Xx1";
                var createResult = await _userManager.CreateAsync(user, defaultPassword);
                if (!createResult.Succeeded)
                {
                    return Error.Validation("User.CreateFailed",
                        string.Join("; ", createResult.Errors.Select(e => e.Description)));
                }

                await _userManager.AddToRoleAsync(user, role.Name!);

                var branchId = employee.BranchId ?? request.BranchId;
                if (branchId.HasValue)
                {
                    _context.UserBranchRoles.Add(new UserBranchRole
                    {
                        UserId = user.Id,
                        BranchId = branchId.Value,
                        RoleName = role.Name!
                    });
                }

                employee.UserId = user.Id;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        // Reload with navigation properties
        var updatedEmployee = await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.JobTitle)
            .Include(e => e.DirectManager)
            .Include(e => e.Branch)
            .Include(e => e.Gender)
            .Include(e => e.MaritalStatus)
            .Include(e => e.ContractType)
            .Include(e => e.Status)
            .FirstAsync(e => e.Id == employee.Id, cancellationToken);

        var dto = new EmployeeDto
        {
            Id = updatedEmployee.Id,
            EmployeeCode = updatedEmployee.EmployeeCode,
            FirstNameAr = updatedEmployee.FirstNameAr,
            LastNameAr = updatedEmployee.LastNameAr,
            FirstNameEn = updatedEmployee.FirstNameEn,
            LastNameEn = updatedEmployee.LastNameEn,
            FullNameAr = updatedEmployee.FullNameAr,
            FullNameEn = updatedEmployee.FullNameEn,
            NationalId = updatedEmployee.NationalId,
            PassportNumber = updatedEmployee.PassportNumber,
            DateOfBirth = updatedEmployee.DateOfBirth,
            GenderId = updatedEmployee.GenderId,
            GenderNameEn = updatedEmployee.Gender?.NameEn,
            GenderNameAr = updatedEmployee.Gender?.NameAr,
            MaritalStatusId = updatedEmployee.MaritalStatusId,
            MaritalStatusNameEn = updatedEmployee.MaritalStatus?.NameEn,
            MaritalStatusNameAr = updatedEmployee.MaritalStatus?.NameAr,
            Email = updatedEmployee.Email,
            PhoneNumber = updatedEmployee.PhoneNumber,
            MobileNumber = updatedEmployee.MobileNumber,
            AddressAr = updatedEmployee.AddressAr,
            AddressEn = updatedEmployee.AddressEn,
            City = updatedEmployee.City,
            Country = updatedEmployee.Country,
            DepartmentId = updatedEmployee.DepartmentId,
            DepartmentNameEn = updatedEmployee.Department?.NameEn ?? string.Empty,
            DepartmentNameAr = updatedEmployee.Department?.NameAr ?? string.Empty,
            JobTitleId = updatedEmployee.JobTitleId,
            JobTitleEn = updatedEmployee.JobTitle?.TitleEn ?? string.Empty,
            JobTitleAr = updatedEmployee.JobTitle?.TitleAr ?? string.Empty,
            DirectManagerId = updatedEmployee.DirectManagerId,
            DirectManagerName = updatedEmployee.DirectManager?.FullNameEn,
            BranchId = updatedEmployee.BranchId,
            BranchName = updatedEmployee.Branch?.NameEn,
            ContractTypeId = updatedEmployee.ContractTypeId,
            ContractTypeNameEn = updatedEmployee.ContractType?.NameEn,
            ContractTypeNameAr = updatedEmployee.ContractType?.NameAr,
            HiringDate = updatedEmployee.HiringDate,
            ProbationPeriodMonths = updatedEmployee.ProbationPeriodMonths,
            ProbationEndDate = updatedEmployee.ProbationEndDate,
            StatusId = updatedEmployee.StatusId,
            StatusNameEn = updatedEmployee.Status?.NameEn,
            StatusNameAr = updatedEmployee.Status?.NameAr,
            CreatedDate = updatedEmployee.CreatedDate.DateTime
        };

        return new GenericResponse<EmployeeDto>
        {
            Success = true,
            Message = "Employee updated successfully",
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
