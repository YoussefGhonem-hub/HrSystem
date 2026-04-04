using ErrorOr;
using HrSystem.Domain.Entities.Account;
using HrSystem.Domain.Entities.Organization;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Department = HrSystem.Domain.Entities.Employee.Department;
using Employee = HrSystem.Domain.Entities.Employee.Employee;
using JobTitle = HrSystem.Domain.Entities.Employee.JobTitle;

namespace HrSystem.Application.Features.Organizations.Commands.CreateOrganizationWithAdmin;

public record CreateOrganizationWithAdminCommand(
    OrganizationInput Organization,
    List<BranchInput> Branches,
    AdminUserInput AdminUser,
    HrManagerUserInput HrManagerUser
) : IRequest<ErrorOr<GenericResponse<OrganizationOnboardingDto>>>;

public record OrganizationInput(
    string NameAr,
    string NameEn,
    string Code,
    string? Industry,
    Guid? SubscriptionPlanId,
    string? LogoUrl,
    string? CommercialRegistrationNumber,
    string? TaxRegistrationNumber,
    string? LegalEntityType,
    string? Email,
    string? PhoneNumber,
    string? Website,
    string? AddressAr,
    string? AddressEn,
    string? City,
    string? Country,
    string? PostalCode,
    string? TimeZone,
    string? Currency,
    string? WeekStartDay,
    string? DefaultLanguage,
    bool IsTrialPeriod = false,
    int TrialDays = 0
);

public record BranchInput(
    string NameAr,
    string NameEn,
    string Code,
    Guid CountryId,
    string? Description,
    string? City,
    string? AddressAr,
    string? AddressEn,
    string? PostalCode,
    double? Latitude,
    double? Longitude,
    string? PhoneNumber,
    string? Email,
    string? Fax,
    string? TimeZone,
    string? Currency,
    string? Language,
    bool IsHeadquarter,
    DateTime? OpeningDate
);

public record AdminUserInput(
    string Email,
    string FullName,
    string Password,
    string? UserName
);

public record HrManagerUserInput(
    string Email,
    string FullName,
    string Password,
    string? UserName
);

public record OrganizationOnboardingDto
{
    public Guid OrganizationId { get; init; }
    public string OrganizationCode { get; init; } = string.Empty;
    public Guid AdminUserId { get; init; }
    public string AdminEmail { get; init; } = string.Empty;
    public Guid HrManagerUserId { get; init; }
    public string HrManagerEmail { get; init; } = string.Empty;
    public List<BranchSummaryDto> Branches { get; init; } = new();
}

public record BranchSummaryDto
{
    public Guid BranchId { get; init; }
    public string NameEn { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public bool IsHeadquarter { get; init; }
}

public class CreateOrganizationWithAdminCommandHandler : IRequestHandler<CreateOrganizationWithAdminCommand, ErrorOr<GenericResponse<OrganizationOnboardingDto>>>
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public CreateOrganizationWithAdminCommandHandler(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<ErrorOr<GenericResponse<OrganizationOnboardingDto>>> Handle(
        CreateOrganizationWithAdminCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Branches == null || request.Branches.Count == 0)
        {
            return Error.Validation("Organization.BranchesRequired", "At least one branch is required");
        }

        var orgCodeExists = await _context.Organizations
            .AnyAsync(o => o.Code == request.Organization.Code, cancellationToken);

        if (orgCodeExists)
        {
            return Error.Conflict("Organization.CodeExists", "Organization code already exists");
        }

        var adminExists = await _userManager.FindByEmailAsync(request.AdminUser.Email);
        if (adminExists != null)
        {
            return Error.Conflict("User.EmailExists", "Admin email already exists");
        }

        var hrManagerExists = await _userManager.FindByEmailAsync(request.HrManagerUser.Email);
        if (hrManagerExists != null)
        {
            return Error.Conflict("User.EmailExists", "HR Manager email already exists");
        }

        if (string.Equals(request.AdminUser.Email, request.HrManagerUser.Email, StringComparison.OrdinalIgnoreCase))
        {
            return Error.Validation("User.DuplicateEmail", "Admin and HR Manager must have different emails");
        }

        if (!await _roleManager.RoleExistsAsync(RoleNames.OrganizationAdmin))
        {
            return Error.NotFound("Role.NotFound", "OrganizationAdmin role not found");
        }

        if (!await _roleManager.RoleExistsAsync(RoleNames.HRManager))
        {
            return Error.NotFound("Role.NotFound", "HRManager role not found");
        }

        if (!await _roleManager.RoleExistsAsync(RoleNames.Employee))
        {
            return Error.NotFound("Role.NotFound", "Employee role not found");
        }

        SubscriptionPlan? plan = null;
        if (request.Organization.SubscriptionPlanId.HasValue)
        {
            plan = await _context.SubscriptionPlans
                .FirstOrDefaultAsync(p => p.Id == request.Organization.SubscriptionPlanId.Value, cancellationToken);

            if (plan == null)
            {
                return Error.NotFound("SubscriptionPlan.NotFound", "Subscription plan not found");
            }
        }

        var now = DateTime.UtcNow;
        var organizationId = Guid.NewGuid();

        var organization = new Organization
        {
            Id = organizationId,
            TenantId = organizationId,
            Code = request.Organization.Code,
            NameAr = request.Organization.NameAr,
            NameEn = request.Organization.NameEn,
            Industry = request.Organization.Industry,
            LogoUrl = request.Organization.LogoUrl,
            CommercialRegistrationNumber = request.Organization.CommercialRegistrationNumber,
            TaxRegistrationNumber = request.Organization.TaxRegistrationNumber,
            LegalEntityType = request.Organization.LegalEntityType,
            Email = request.Organization.Email,
            PhoneNumber = request.Organization.PhoneNumber,
            Website = request.Organization.Website,
            AddressAr = request.Organization.AddressAr,
            AddressEn = request.Organization.AddressEn,
            City = request.Organization.City,
            Country = request.Organization.Country,
            PostalCode = request.Organization.PostalCode,
            SubscriptionPlanId = plan?.Id,
            SubscriptionStartDate = now,
            SubscriptionEndDate = request.Organization.TrialDays > 0 ? now.AddDays(request.Organization.TrialDays) : null,
            IsActive = true,
            IsTrialPeriod = request.Organization.IsTrialPeriod,
            TrialEndDate = request.Organization.TrialDays > 0 ? now.AddDays(request.Organization.TrialDays) : null,
            TimeZone = request.Organization.TimeZone ?? "Egypt Standard Time",
            Currency = request.Organization.Currency ?? "EGP",
            WeekStartDay = request.Organization.WeekStartDay,
            DefaultLanguage = request.Organization.DefaultLanguage ?? "en"
        };

        var branchCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var branchInput in request.Branches)
        {
            if (!branchCodes.Add(branchInput.Code))
            {
                return Error.Validation("Branch.DuplicateCode", "Duplicate branch code in request");
            }
        }

        var existingCodes = await _context.Branches
            .Where(b => branchCodes.Contains(b.Code))
            .Select(b => b.Code)
            .ToListAsync(cancellationToken);

        if (existingCodes.Count > 0)
        {
            return Error.Conflict("Branch.CodeExists", $"Branch code(s) already exist: {string.Join(", ", existingCodes)}");
        }

        var branches = request.Branches.Select(input => new Branch
        {
            Id = Guid.NewGuid(),
            OrganizationId = organization.Id,
            TenantId = organization.Id,
            NameAr = input.NameAr,
            NameEn = input.NameEn,
            Code = input.Code,
            Description = input.Description,
            CountryId = input.CountryId,
            City = input.City,
            AddressAr = input.AddressAr,
            AddressEn = input.AddressEn,
            PostalCode = input.PostalCode,
            Latitude = input.Latitude,
            Longitude = input.Longitude,
            PhoneNumber = input.PhoneNumber,
            Email = input.Email,
            Fax = input.Fax,
            TimeZone = input.TimeZone ?? organization.TimeZone,
            Currency = input.Currency ?? organization.Currency,
            Language = input.Language,
            IsHeadquarter = input.IsHeadquarter,
            IsActive = true,
            OpeningDate = input.OpeningDate,
            CreatedDate = DateTimeOffset.UtcNow
        }).ToList();

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = string.IsNullOrWhiteSpace(request.AdminUser.UserName) ? request.AdminUser.Email : request.AdminUser.UserName,
            Email = request.AdminUser.Email,
            EmailConfirmed = true,
            FullName = request.AdminUser.FullName,
            IsActive = true,
            OrganizationId = organization.Id,
            CreatedDate = DateTimeOffset.UtcNow
        };

        var hrManagerUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = string.IsNullOrWhiteSpace(request.HrManagerUser.UserName) ? request.HrManagerUser.Email : request.HrManagerUser.UserName,
            Email = request.HrManagerUser.Email,
            EmailConfirmed = true,
            FullName = request.HrManagerUser.FullName,
            IsActive = true,
            OrganizationId = organization.Id,
            BranchId = branches.OrderByDescending(b => b.IsHeadquarter).ThenBy(b => b.NameEn).Select(b => (Guid?)b.Id).FirstOrDefault(),
            CreatedDate = DateTimeOffset.UtcNow
        };

        await using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);

        await _context.Organizations.AddAsync(organization, cancellationToken);
        await _context.Branches.AddRangeAsync(branches, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var createUserResult = await _userManager.CreateAsync(user, request.AdminUser.Password);
        if (!createUserResult.Succeeded)
        {
            return Error.Validation("User.CreateFailed", string.Join("; ", createUserResult.Errors.Select(e => e.Description)));
        }

        var roleResult = await _userManager.AddToRoleAsync(user, RoleNames.OrganizationAdmin);
        if (!roleResult.Succeeded)
        {
            return Error.Validation("User.RoleAssignFailed", string.Join("; ", roleResult.Errors.Select(e => e.Description)));
        }

        var createHrManagerResult = await _userManager.CreateAsync(hrManagerUser, request.HrManagerUser.Password);
        if (!createHrManagerResult.Succeeded)
        {
            return Error.Validation("User.CreateFailed", string.Join("; ", createHrManagerResult.Errors.Select(e => e.Description)));
        }

        var hrManagerRoleResult = await _userManager.AddToRoleAsync(hrManagerUser, RoleNames.HRManager);
        if (!hrManagerRoleResult.Succeeded)
        {
            return Error.Validation("User.RoleAssignFailed", string.Join("; ", hrManagerRoleResult.Errors.Select(e => e.Description)));
        }

        var employeeRoleResult = await _userManager.AddToRoleAsync(hrManagerUser, RoleNames.Employee);
        if (!employeeRoleResult.Succeeded)
        {
            return Error.Validation("User.RoleAssignFailed", string.Join("; ", employeeRoleResult.Errors.Select(e => e.Description)));
        }

        // ── Create Employee records for OrgAdmin and HRManager ──
        var headquarterBranch = branches.OrderByDescending(b => b.IsHeadquarter).ThenBy(b => b.NameEn).First();

        // Ensure at least one department and job title exist for the new org
        var defaultDepartment = await _context.Departments
            .Where(d => d.TenantId == organization.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (defaultDepartment == null)
        {
            var defaultDepts = new[]
            {
                new Department { NameAr = "الإدارة", NameEn = "Management", Code = "MGMT", Description = "General Management", OrganizationId = organization.Id, TenantId = organization.Id, BranchId = headquarterBranch.Id, IsActive = true, SortOrder = 1, CreatedDate = DateTimeOffset.UtcNow },
                new Department { NameAr = "الموارد البشرية", NameEn = "Human Resources", Code = "HR", Description = "Human Resources Department", OrganizationId = organization.Id, TenantId = organization.Id, BranchId = headquarterBranch.Id, IsActive = true, SortOrder = 2, CreatedDate = DateTimeOffset.UtcNow },
                new Department { NameAr = "تكنولوجيا المعلومات", NameEn = "Information Technology", Code = "IT", Description = "IT Department", OrganizationId = organization.Id, TenantId = organization.Id, BranchId = headquarterBranch.Id, IsActive = true, SortOrder = 3, CreatedDate = DateTimeOffset.UtcNow },
                new Department { NameAr = "المالية", NameEn = "Finance", Code = "FIN", Description = "Finance Department", OrganizationId = organization.Id, TenantId = organization.Id, BranchId = headquarterBranch.Id, IsActive = true, SortOrder = 4, CreatedDate = DateTimeOffset.UtcNow },
                new Department { NameAr = "العمليات", NameEn = "Operations", Code = "OPS", Description = "Operations Department", OrganizationId = organization.Id, TenantId = organization.Id, BranchId = headquarterBranch.Id, IsActive = true, SortOrder = 5, CreatedDate = DateTimeOffset.UtcNow },
            };
            await _context.Departments.AddRangeAsync(defaultDepts, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            defaultDepartment = defaultDepts[0]; // Management dept
        }

        var hrDepartment = await _context.Departments
            .Where(d => d.TenantId == organization.Id && d.Code == "HR")
            .FirstOrDefaultAsync(cancellationToken) ?? defaultDepartment;

        var defaultJobTitle = await _context.JobTitles
            .Where(j => j.TenantId == organization.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (defaultJobTitle == null)
        {
            var defaultJobs = new[]
            {
                new JobTitle { TitleAr = "مدير عام", TitleEn = "General Manager", Code = "GM", Level = 1, MinSalary = 30000, MaxSalary = 80000, OrganizationId = organization.Id, TenantId = organization.Id, BranchId = headquarterBranch.Id, IsActive = true, SortOrder = 1, CreatedDate = DateTimeOffset.UtcNow },
                new JobTitle { TitleAr = "مدير موارد بشرية", TitleEn = "HR Manager", Code = "HR-MGR", Level = 3, MinSalary = 15000, MaxSalary = 35000, OrganizationId = organization.Id, TenantId = organization.Id, BranchId = headquarterBranch.Id, IsActive = true, SortOrder = 2, CreatedDate = DateTimeOffset.UtcNow },
                new JobTitle { TitleAr = "موظف", TitleEn = "Employee", Code = "EMP", Level = 5, MinSalary = 5000, MaxSalary = 15000, OrganizationId = organization.Id, TenantId = organization.Id, BranchId = headquarterBranch.Id, IsActive = true, SortOrder = 3, CreatedDate = DateTimeOffset.UtcNow },
            };
            await _context.JobTitles.AddRangeAsync(defaultJobs, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            defaultJobTitle = defaultJobs[0]; // General Manager title
        }

        var hrJobTitle = await _context.JobTitles
            .Where(j => j.TenantId == organization.Id && j.Code == "HR-MGR")
            .FirstOrDefaultAsync(cancellationToken) ?? defaultJobTitle;

        var adminNameParts = (request.AdminUser.FullName ?? "Org Admin").Split(' ', 2);
        var adminEmployee = new Employee
        {
            EmployeeCode = "EMP-0001",
            FirstNameAr = adminNameParts[0],
            LastNameAr = adminNameParts.Length > 1 ? adminNameParts[1] : "",
            FirstNameEn = adminNameParts[0],
            LastNameEn = adminNameParts.Length > 1 ? adminNameParts[1] : "",
            NationalId = $"ADMIN-{organization.Code}",
            DateOfBirth = DateTime.UtcNow.AddYears(-35),
            GenderId = GenderIds.Male,
            MaritalStatusId = MaritalStatusIds.Single,
            Email = request.AdminUser.Email,
            PhoneNumber = request.AdminUser.Email,
            AddressAr = organization.AddressAr ?? "—",
            AddressEn = organization.AddressEn,
            City = organization.City,
            Country = organization.Country,
            DepartmentId = defaultDepartment.Id,
            JobTitleId = defaultJobTitle.Id,
            BranchId = headquarterBranch.Id,
            ContractTypeId = ContractTypeIds.Permanent,
            StatusId = EmployeeStatusIds.Active,
            HiringDate = DateTime.UtcNow,
            TenantId = organization.Id,
            UserId = user.Id,
            CreatedDate = DateTimeOffset.UtcNow
        };
        _context.Employees.Add(adminEmployee);
        await _context.SaveChangesAsync(cancellationToken);
        user.EmployeeId = adminEmployee.Id;
        await _userManager.UpdateAsync(user);

        var hrNameParts = (request.HrManagerUser.FullName ?? "HR Manager").Split(' ', 2);
        var hrEmployee = new Employee
        {
            EmployeeCode = "EMP-0002",
            FirstNameAr = hrNameParts[0],
            LastNameAr = hrNameParts.Length > 1 ? hrNameParts[1] : "",
            FirstNameEn = hrNameParts[0],
            LastNameEn = hrNameParts.Length > 1 ? hrNameParts[1] : "",
            NationalId = $"HR-{organization.Code}",
            DateOfBirth = DateTime.UtcNow.AddYears(-30),
            GenderId = GenderIds.Male,
            MaritalStatusId = MaritalStatusIds.Single,
            Email = request.HrManagerUser.Email,
            PhoneNumber = request.HrManagerUser.Email,
            AddressAr = organization.AddressAr ?? "—",
            AddressEn = organization.AddressEn,
            City = organization.City,
            Country = organization.Country,
            DepartmentId = hrDepartment.Id,
            JobTitleId = hrJobTitle.Id,
            DirectManagerId = adminEmployee.Id,
            BranchId = headquarterBranch.Id,
            ContractTypeId = ContractTypeIds.Permanent,
            StatusId = EmployeeStatusIds.Active,
            HiringDate = DateTime.UtcNow,
            TenantId = organization.Id,
            UserId = hrManagerUser.Id,
            CreatedDate = DateTimeOffset.UtcNow
        };
        _context.Employees.Add(hrEmployee);
        await _context.SaveChangesAsync(cancellationToken);
        hrManagerUser.EmployeeId = hrEmployee.Id;
        await _userManager.UpdateAsync(hrManagerUser);

        var branchRoles = branches.Select(branch => new UserBranchRole
        {
            UserId = user.Id,
            BranchId = branch.Id,
            RoleName = RoleNames.OrganizationAdmin
        });

        var hrManagerBranchRoles = branches.SelectMany(branch => new[]
        {
            new UserBranchRole
            {
                UserId = hrManagerUser.Id,
                BranchId = branch.Id,
                RoleName = RoleNames.HRManager
            },
            new UserBranchRole
            {
                UserId = hrManagerUser.Id,
                BranchId = branch.Id,
                RoleName = RoleNames.Employee
            }
        });

        await _context.UserBranchRoles.AddRangeAsync(branchRoles, cancellationToken);
        await _context.UserBranchRoles.AddRangeAsync(hrManagerBranchRoles, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        await tx.CommitAsync(cancellationToken);

        var dto = new OrganizationOnboardingDto
        {
            OrganizationId = organization.Id,
            OrganizationCode = organization.Code,
            AdminUserId = user.Id,
            AdminEmail = user.Email ?? string.Empty,
            HrManagerUserId = hrManagerUser.Id,
            HrManagerEmail = hrManagerUser.Email ?? string.Empty,
            Branches = branches.Select(b => new BranchSummaryDto
            {
                BranchId = b.Id,
                NameEn = b.NameEn,
                Code = b.Code,
                IsHeadquarter = b.IsHeadquarter
            }).ToList()
        };

        return new GenericResponse<OrganizationOnboardingDto>
        {
            Success = true,
            Message = "Organization created successfully",
            Data = dto
        };
    }
}
