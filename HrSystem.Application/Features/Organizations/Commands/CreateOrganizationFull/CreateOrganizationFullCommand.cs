using ErrorOr;
using HrSystem.Domain.Entities.Account;
using HrSystem.Domain.Entities.Employee;
using HrSystem.Domain.Entities.Organization;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Organizations.Commands.CreateOrganizationFull;

#region Command & Input Records

/// <summary>
/// Creates a complete organization with all nested entities in a single transaction.
/// Supports full onboarding: company profile, branches, departments, job titles, schedules, holidays, and admin user.
/// </summary>
public record CreateOrganizationFullCommand(
    OrganizationProfileInput Organization,
    List<BranchFullInput> Branches,
    List<DepartmentInput>? Departments,
    List<JobTitleInput>? JobTitles,
    OrganizationAdminUserInput AdminUser,
    OrganizationHrManagerUserInput HrManagerUser
) : IRequest<ErrorOr<GenericResponse<OrganizationFullDto>>>;

/// <summary>
/// Company profile information (org-company-info-tab)
/// </summary>
public record OrganizationProfileInput(
    // Basic Info
    string NameAr,
    string NameEn,
    string Code,
    string? Industry,
    string? LogoUrl,
    
    // Legal Info
    string? CommercialRegistrationNumber,
    string? TaxRegistrationNumber,
    string? LegalEntityType,
    
    // Contact Info
    string? Email,
    string? PhoneNumber,
    string? SecondaryPhoneNumber,
    string? Website,
    string? AddressAr,
    string? AddressEn,
    string? City,
    string? Country,
    string? PostalCode,
    
    // Settings
    string? TimeZone,
    string? Currency,
    string? WeekStartDay,
    string? DefaultLanguage,
    
    // Subscription
    Guid? SubscriptionPlanId,
    bool IsTrialPeriod = false,
    int TrialDays = 0
);

/// <summary>
/// Branch input with nested schedules and holidays (org-branches-tab)
/// </summary>
public record BranchFullInput(
    // Basic Info
    string NameAr,
    string NameEn,
    string Code,
    string? Description,
    
    // Location
    Guid CountryId,
    string? City,
    string? AddressAr,
    string? AddressEn,
    string? PostalCode,
    double? Latitude,
    double? Longitude,
    
    // Contact
    string? PhoneNumber,
    string? Email,
    string? Fax,
    
    // Settings
    string? TimeZone,
    string? Currency,
    string? Language,
    
    // Status
    bool IsHeadquarter,
    DateTime? OpeningDate,

    // Nested: Work Schedules (org-work-schedule-tab)
    List<BranchWorkScheduleInput>? WorkSchedules,

    // Nested: Holidays (org-holidays-tab)
    List<BranchHolidayInput>? Holidays
);

/// <summary>
/// Work schedule configuration for a branch
/// </summary>
public record BranchWorkScheduleInput(
    string Name,
    TimeSpan StartTime,
    TimeSpan EndTime,
    TimeSpan? BreakDuration,
    int WorkingHoursPerDay,
    int WorkingDaysPerWeek,
    TimeSpan? GracePeriodLate,
    TimeSpan? GracePeriodEarlyLeave,
    bool IsSunday,
    bool IsMonday,
    bool IsTuesday,
    bool IsWednesday,
    bool IsThursday,
    bool IsFriday,
    bool IsSaturday,
    bool IsDefault,
    string? TimeZone
);

/// <summary>
/// Holiday definition for a branch
/// </summary>
public record BranchHolidayInput(
    string NameAr,
    string NameEn,
    string? Description,
    DateTime Date,
    int Year,
    bool IsRecurring,
    int? RecurringMonth,
    int? RecurringDay,
    HolidayType Type
);

/// <summary>
/// Department input (org-structure-tab)
/// </summary>
public record DepartmentInput(
    string NameAr,
    string NameEn,
    string Code,
    string? Description,
    string? ParentDepartmentCode, // Reference by code for linking
    int SortOrder
);

/// <summary>
/// Job title input (org-structure-tab)
/// </summary>
public record JobTitleInput(
    string TitleAr,
    string TitleEn,
    string Code,
    string? Description,
    int Level,
    decimal MinSalary,
    decimal MaxSalary,
    int SortOrder
);

/// <summary>
/// Admin user credentials
/// </summary>
public record OrganizationAdminUserInput(
    string Email,
    string FullName,
    string Password,
    string? UserName
);

/// <summary>
/// Initial HR manager user credentials
/// </summary>
public record OrganizationHrManagerUserInput(
    string Email,
    string FullName,
    string Password,
    string? UserName
);

#endregion

#region Response DTOs

public record OrganizationFullDto
{
    public Guid OrganizationId { get; init; }
    public string OrganizationCode { get; init; } = string.Empty;
    public string OrganizationNameEn { get; init; } = string.Empty;
    public Guid AdminUserId { get; init; }
    public string AdminEmail { get; init; } = string.Empty;
    public Guid HrManagerUserId { get; init; }
    public string HrManagerEmail { get; init; } = string.Empty;
    public List<BranchFullDto> Branches { get; init; } = new();
    public List<DepartmentDto> Departments { get; init; } = new();
    public List<JobTitleDto> JobTitles { get; init; } = new();
}

public record BranchFullDto
{
    public Guid BranchId { get; init; }
    public string NameEn { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public bool IsHeadquarter { get; init; }
    public List<WorkScheduleDto> WorkSchedules { get; init; } = new();
    public List<HolidayDto> Holidays { get; init; } = new();
}

public record WorkScheduleDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsDefault { get; init; }
}

public record HolidayDto
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = string.Empty;
    public DateTime Date { get; init; }
}

public record DepartmentDto
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public Guid? ParentDepartmentId { get; init; }
}

public record JobTitleDto
{
    public Guid Id { get; init; }
    public string TitleEn { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public int Level { get; init; }
}

#endregion

#region Handler

public class CreateOrganizationFullCommandHandler 
    : IRequestHandler<CreateOrganizationFullCommand, ErrorOr<GenericResponse<OrganizationFullDto>>>
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public CreateOrganizationFullCommandHandler(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<ErrorOr<GenericResponse<OrganizationFullDto>>> Handle(
        CreateOrganizationFullCommand request,
        CancellationToken cancellationToken)
    {
        // ─────────────────────────────────────────────────────────────────
        // 1. Validation
        // ─────────────────────────────────────────────────────────────────
        
        if (request.Branches == null || request.Branches.Count == 0)
            return Error.Validation("Organization.BranchesRequired", "At least one branch is required");

        // Check organization code uniqueness
        var orgCodeExists = await _context.Organizations
            .AnyAsync(o => o.Code == request.Organization.Code, cancellationToken);
        if (orgCodeExists)
            return Error.Conflict("Organization.CodeExists", $"Organization code '{request.Organization.Code}' already exists");

        // Check admin email uniqueness
        var adminExists = await _userManager.FindByEmailAsync(request.AdminUser.Email);
        if (adminExists != null)
            return Error.Conflict("User.EmailExists", "Admin email already exists");

        // Check HR manager email uniqueness
        var hrManagerExists = await _userManager.FindByEmailAsync(request.HrManagerUser.Email);
        if (hrManagerExists != null)
            return Error.Conflict("User.EmailExists", "HR Manager email already exists");

        if (string.Equals(request.AdminUser.Email, request.HrManagerUser.Email, StringComparison.OrdinalIgnoreCase))
            return Error.Validation("User.DuplicateEmail", "Admin and HR Manager must have different emails");

        // Check OrganizationAdmin role exists
        if (!await _roleManager.RoleExistsAsync(RoleNames.OrganizationAdmin))
            return Error.NotFound("Role.NotFound", "OrganizationAdmin role not found");

        // Check HRManager and Employee roles exist
        if (!await _roleManager.RoleExistsAsync(RoleNames.HRManager))
            return Error.NotFound("Role.NotFound", "HRManager role not found");

        if (!await _roleManager.RoleExistsAsync(RoleNames.Employee))
            return Error.NotFound("Role.NotFound", "Employee role not found");

        // Validate branch codes uniqueness within request
        var branchCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var branch in request.Branches)
        {
            if (!branchCodes.Add(branch.Code))
                return Error.Validation("Branch.DuplicateCode", $"Duplicate branch code '{branch.Code}' in request");
        }

        // Check branch codes don't exist in database
        var existingBranchCodes = await _context.Branches
            .Where(b => branchCodes.Contains(b.Code))
            .Select(b => b.Code)
            .ToListAsync(cancellationToken);
        if (existingBranchCodes.Count > 0)
            return Error.Conflict("Branch.CodeExists", $"Branch code(s) already exist: {string.Join(", ", existingBranchCodes)}");

        // Validate department codes uniqueness
        if (request.Departments?.Count > 0)
        {
            var deptCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var dept in request.Departments)
            {
                if (!deptCodes.Add(dept.Code))
                    return Error.Validation("Department.DuplicateCode", $"Duplicate department code '{dept.Code}' in request");
            }
        }

        // Validate job title codes uniqueness
        if (request.JobTitles?.Count > 0)
        {
            var titleCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var title in request.JobTitles)
            {
                if (!titleCodes.Add(title.Code))
                    return Error.Validation("JobTitle.DuplicateCode", $"Duplicate job title code '{title.Code}' in request");
            }
        }

        // Validate subscription plan if provided
        SubscriptionPlan? plan = null;
        if (request.Organization.SubscriptionPlanId.HasValue)
        {
            plan = await _context.SubscriptionPlans
                .FirstOrDefaultAsync(p => p.Id == request.Organization.SubscriptionPlanId.Value, cancellationToken);
            if (plan == null)
                return Error.NotFound("SubscriptionPlan.NotFound", "Subscription plan not found");
        }

        // Validate country IDs exist
        var countryIds = request.Branches.Select(b => b.CountryId).Distinct().ToList();
        var existingCountries = await _context.Countries
            .Where(c => countryIds.Contains(c.Id))
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);
        var missingCountries = countryIds.Except(existingCountries).ToList();
        if (missingCountries.Count > 0)
            return Error.NotFound("Country.NotFound", $"Country ID(s) not found: {string.Join(", ", missingCountries)}");

        // ─────────────────────────────────────────────────────────────────
        // 2. Create Entities
        // ─────────────────────────────────────────────────────────────────

        var now = DateTime.UtcNow;
        var orgId = Guid.NewGuid();

        // Create Organization
        var organization = new Organization
        {
            Id = orgId,
            TenantId = orgId,
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
            Country = request.Organization.Country ?? "Egypt",
            PostalCode = request.Organization.PostalCode,
            SubscriptionPlanId = plan?.Id,
            SubscriptionStartDate = now,
            SubscriptionEndDate = request.Organization.TrialDays > 0 ? now.AddDays(request.Organization.TrialDays) : null,
            IsActive = true,
            IsTrialPeriod = request.Organization.IsTrialPeriod,
            TrialEndDate = request.Organization.TrialDays > 0 ? now.AddDays(request.Organization.TrialDays) : null,
            TimeZone = request.Organization.TimeZone ?? "Egypt Standard Time",
            Currency = request.Organization.Currency ?? "EGP",
            WeekStartDay = request.Organization.WeekStartDay ?? "Sunday",
            DefaultLanguage = request.Organization.DefaultLanguage ?? "en",
            CreatedDate = DateTimeOffset.UtcNow
        };

        // Create Branches with Work Schedules and Holidays
        var branches = new List<Branch>();
        var allWorkSchedules = new List<BranchWorkSchedule>();
        var allHolidays = new List<BranchHoliday>();

        foreach (var branchInput in request.Branches)
        {
            var branchId = Guid.NewGuid();
            
            var branch = new Branch
            {
                Id = branchId,
                TenantId = orgId,
                OrganizationId = orgId,
                NameAr = branchInput.NameAr,
                NameEn = branchInput.NameEn,
                Code = branchInput.Code,
                Description = branchInput.Description,
                CountryId = branchInput.CountryId,
                City = branchInput.City,
                AddressAr = branchInput.AddressAr,
                AddressEn = branchInput.AddressEn,
                PostalCode = branchInput.PostalCode,
                Latitude = branchInput.Latitude,
                Longitude = branchInput.Longitude,
                PhoneNumber = branchInput.PhoneNumber,
                Email = branchInput.Email,
                Fax = branchInput.Fax,
                TimeZone = branchInput.TimeZone ?? organization.TimeZone,
                Currency = branchInput.Currency ?? organization.Currency,
                Language = branchInput.Language ?? "ar",
                IsHeadquarter = branchInput.IsHeadquarter,
                IsActive = true,
                OpeningDate = branchInput.OpeningDate,
                CreatedDate = DateTimeOffset.UtcNow
            };
            branches.Add(branch);

            // Create work schedules for this branch
            if (branchInput.WorkSchedules?.Count > 0)
            {
                foreach (var scheduleInput in branchInput.WorkSchedules)
                {
                    var schedule = new BranchWorkSchedule
                    {
                        Id = Guid.NewGuid(),
                        TenantId = orgId,
                        BranchId = branchId,
                        Name = scheduleInput.Name,
                        StartTime = scheduleInput.StartTime,
                        EndTime = scheduleInput.EndTime,
                        BreakDuration = scheduleInput.BreakDuration,
                        WorkingHoursPerDay = scheduleInput.WorkingHoursPerDay,
                        WorkingDaysPerWeek = scheduleInput.WorkingDaysPerWeek,
                        GracePeriodLate = scheduleInput.GracePeriodLate,
                        GracePeriodEarlyLeave = scheduleInput.GracePeriodEarlyLeave,
                        IsSunday = scheduleInput.IsSunday,
                        IsMonday = scheduleInput.IsMonday,
                        IsTuesday = scheduleInput.IsTuesday,
                        IsWednesday = scheduleInput.IsWednesday,
                        IsThursday = scheduleInput.IsThursday,
                        IsFriday = scheduleInput.IsFriday,
                        IsSaturday = scheduleInput.IsSaturday,
                        IsDefault = scheduleInput.IsDefault,
                        TimeZone = scheduleInput.TimeZone ?? branch.TimeZone,
                        IsActive = true,
                        CreatedDate = DateTimeOffset.UtcNow
                    };
                    allWorkSchedules.Add(schedule);
                }
            }
            else
            {
                // Create default schedule if none provided
                var defaultSchedule = new BranchWorkSchedule
                {
                    Id = Guid.NewGuid(),
                    TenantId = orgId,
                    BranchId = branchId,
                    Name = "Default Schedule",
                    StartTime = new TimeSpan(9, 0, 0),
                    EndTime = new TimeSpan(17, 0, 0),
                    BreakDuration = new TimeSpan(1, 0, 0),
                    WorkingHoursPerDay = 8,
                    WorkingDaysPerWeek = 5,
                    GracePeriodLate = new TimeSpan(0, 15, 0),
                    GracePeriodEarlyLeave = new TimeSpan(0, 15, 0),
                    IsSunday = true,
                    IsMonday = true,
                    IsTuesday = true,
                    IsWednesday = true,
                    IsThursday = true,
                    IsFriday = false,
                    IsSaturday = false,
                    IsDefault = true,
                    TimeZone = branch.TimeZone,
                    IsActive = true,
                    CreatedDate = DateTimeOffset.UtcNow
                };
                allWorkSchedules.Add(defaultSchedule);
            }

            // Create holidays for this branch
            if (branchInput.Holidays?.Count > 0)
            {
                foreach (var holidayInput in branchInput.Holidays)
                {
                    var holiday = new BranchHoliday
                    {
                        Id = Guid.NewGuid(),
                        TenantId = orgId,
                        BranchId = branchId,
                        NameAr = holidayInput.NameAr,
                        NameEn = holidayInput.NameEn,
                        Description = holidayInput.Description,
                        Date = holidayInput.Date,
                        Year = holidayInput.Year,
                        IsRecurring = holidayInput.IsRecurring,
                        RecurringMonth = holidayInput.RecurringMonth,
                        RecurringDay = holidayInput.RecurringDay,
                        Type = holidayInput.Type,
                        IsActive = true,
                        CreatedDate = DateTimeOffset.UtcNow
                    };
                    allHolidays.Add(holiday);
                }
            }
        }

        // Create Departments
        var departments = new List<Department>();
        var deptCodeToId = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

        if (request.Departments?.Count > 0)
        {
            // First pass: create all departments without parent links
            foreach (var deptInput in request.Departments)
            {
                var deptId = Guid.NewGuid();
                deptCodeToId[deptInput.Code] = deptId;
                
                var dept = new Department
                {
                    Id = deptId,
                    TenantId = orgId,
                    OrganizationId = orgId,
                    NameAr = deptInput.NameAr,
                    NameEn = deptInput.NameEn,
                    Code = deptInput.Code,
                    Description = deptInput.Description,
                    SortOrder = deptInput.SortOrder,
                    IsActive = true,
                    CreatedDate = DateTimeOffset.UtcNow
                };
                departments.Add(dept);
            }

            // Second pass: link parent departments
            foreach (var deptInput in request.Departments)
            {
                if (!string.IsNullOrEmpty(deptInput.ParentDepartmentCode))
                {
                    if (deptCodeToId.TryGetValue(deptInput.ParentDepartmentCode, out var parentId))
                    {
                        var dept = departments.First(d => d.Code == deptInput.Code);
                        dept.ParentDepartmentId = parentId;
                    }
                    else
                    {
                        return Error.Validation("Department.InvalidParent", 
                            $"Parent department code '{deptInput.ParentDepartmentCode}' not found for department '{deptInput.Code}'");
                    }
                }
            }
        }

        // Create Job Titles
        var jobTitles = new List<JobTitle>();
        if (request.JobTitles?.Count > 0)
        {
            foreach (var titleInput in request.JobTitles)
            {
                var title = new JobTitle
                {
                    Id = Guid.NewGuid(),
                    TenantId = orgId,
                    OrganizationId = orgId,
                    TitleAr = titleInput.TitleAr,
                    TitleEn = titleInput.TitleEn,
                    Code = titleInput.Code,
                    Description = titleInput.Description,
                    Level = titleInput.Level,
                    MinSalary = titleInput.MinSalary,
                    MaxSalary = titleInput.MaxSalary,
                    SortOrder = titleInput.SortOrder,
                    IsActive = true,
                    CreatedDate = DateTimeOffset.UtcNow
                };
                jobTitles.Add(title);
            }
        }

        // Create Admin User
        var adminUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = string.IsNullOrWhiteSpace(request.AdminUser.UserName) 
                ? request.AdminUser.Email 
                : request.AdminUser.UserName,
            Email = request.AdminUser.Email,
            EmailConfirmed = true,
            FullName = request.AdminUser.FullName,
            IsActive = true,
            OrganizationId = orgId,
            CreatedDate = DateTimeOffset.UtcNow
        };

        // Create Initial HR Manager User
        var hrManagerUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = string.IsNullOrWhiteSpace(request.HrManagerUser.UserName)
                ? request.HrManagerUser.Email
                : request.HrManagerUser.UserName,
            Email = request.HrManagerUser.Email,
            EmailConfirmed = true,
            FullName = request.HrManagerUser.FullName,
            IsActive = true,
            OrganizationId = orgId,
            BranchId = branches.OrderByDescending(b => b.IsHeadquarter).ThenBy(b => b.NameEn).Select(b => (Guid?)b.Id).FirstOrDefault(),
            CreatedDate = DateTimeOffset.UtcNow
        };

        // ─────────────────────────────────────────────────────────────────
        // 3. Persist in Transaction
        // ─────────────────────────────────────────────────────────────────

        await using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // Save organization
            await _context.Organizations.AddAsync(organization, cancellationToken);
            
            // Save branches
            await _context.Branches.AddRangeAsync(branches, cancellationToken);

            // Save work schedules
            await _context.BranchWorkSchedules.AddRangeAsync(allWorkSchedules, cancellationToken);
            
            // Save holidays
            await _context.BranchHolidays.AddRangeAsync(allHolidays, cancellationToken);
            
            // Save departments
            if (departments.Count > 0)
                await _context.Departments.AddRangeAsync(departments, cancellationToken);
            
            // Save job titles
            if (jobTitles.Count > 0)
                await _context.JobTitles.AddRangeAsync(jobTitles, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            // Create user with Identity
            var createAdminResult = await _userManager.CreateAsync(adminUser, request.AdminUser.Password);
            if (!createAdminResult.Succeeded)
            {
                return Error.Validation("User.CreateFailed", 
                    string.Join("; ", createAdminResult.Errors.Select(e => e.Description)));
            }

            var createHrManagerResult = await _userManager.CreateAsync(hrManagerUser, request.HrManagerUser.Password);
            if (!createHrManagerResult.Succeeded)
            {
                return Error.Validation("User.CreateFailed",
                    string.Join("; ", createHrManagerResult.Errors.Select(e => e.Description)));
            }

            // Assign roles
            var adminRoleResult = await _userManager.AddToRoleAsync(adminUser, RoleNames.OrganizationAdmin);
            if (!adminRoleResult.Succeeded)
            {
                return Error.Validation("User.RoleAssignFailed",
                    string.Join("; ", adminRoleResult.Errors.Select(e => e.Description)));
            }

            var hrManagerRoleResult = await _userManager.AddToRoleAsync(hrManagerUser, RoleNames.HRManager);
            if (!hrManagerRoleResult.Succeeded)
            {
                return Error.Validation("User.RoleAssignFailed",
                    string.Join("; ", hrManagerRoleResult.Errors.Select(e => e.Description)));
            }

            var employeeRoleResult = await _userManager.AddToRoleAsync(hrManagerUser, RoleNames.Employee);
            if (!employeeRoleResult.Succeeded)
            {
                return Error.Validation("User.RoleAssignFailed",
                    string.Join("; ", employeeRoleResult.Errors.Select(e => e.Description)));
            }

            // Create UserBranchRole for all branches
            var adminBranchRoles = branches.Select(branch => new UserBranchRole
            {
                UserId = adminUser.Id,
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

            await _context.UserBranchRoles.AddRangeAsync(adminBranchRoles, cancellationToken);
            await _context.UserBranchRoles.AddRangeAsync(hrManagerBranchRoles, cancellationToken);
            
            await _context.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }

        // ─────────────────────────────────────────────────────────────────
        // 4. Build Response DTO
        // ─────────────────────────────────────────────────────────────────

        var dto = new OrganizationFullDto
        {
            OrganizationId = organization.Id,
            OrganizationCode = organization.Code,
            OrganizationNameEn = organization.NameEn,
            AdminUserId = adminUser.Id,
            AdminEmail = adminUser.Email ?? string.Empty,
            HrManagerUserId = hrManagerUser.Id,
            HrManagerEmail = hrManagerUser.Email ?? string.Empty,
            Branches = branches.Select(b => new BranchFullDto
            {
                BranchId = b.Id,
                NameEn = b.NameEn,
                Code = b.Code,
                IsHeadquarter = b.IsHeadquarter,
                WorkSchedules = allWorkSchedules
                    .Where(s => s.BranchId == b.Id)
                    .Select(s => new WorkScheduleDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        IsDefault = s.IsDefault
                    }).ToList(),
                Holidays = allHolidays
                    .Where(h => h.BranchId == b.Id)
                    .Select(h => new HolidayDto
                    {
                        Id = h.Id,
                        NameEn = h.NameEn,
                        Date = h.Date
                    }).ToList()
            }).ToList(),
            Departments = departments.Select(d => new DepartmentDto
            {
                Id = d.Id,
                NameEn = d.NameEn,
                Code = d.Code,
                ParentDepartmentId = d.ParentDepartmentId
            }).ToList(),
            JobTitles = jobTitles.Select(j => new JobTitleDto
            {
                Id = j.Id,
                TitleEn = j.TitleEn,
                Code = j.Code,
                Level = j.Level
            }).ToList()
        };

        return new GenericResponse<OrganizationFullDto>
        {
            Success = true,
            Message = "Organization created successfully with all components",
            Data = dto
        };
    }
}

#endregion
