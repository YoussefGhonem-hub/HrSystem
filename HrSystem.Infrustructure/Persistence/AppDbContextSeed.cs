using HrSystem.Domain.Common;
using HrSystem.Domain.Entities.Account;
using HrSystem.Domain.Entities.Attendance;
using HrSystem.Domain.Entities.Employee;
using HrSystem.Domain.Entities.Leave;
using HrSystem.Domain.Entities.Lifecycle;
using HrSystem.Domain.Entities.Organization;
using HrSystem.Domain.Entities.Payroll;
using HrSystem.Domain.Entities.Performance;
using HrSystem.Domain.Enums;
using HrSystem.Shared.Constants;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using static HrSystem.Infrustructure.Persistence.SeedData.SeedDataDtos;

namespace HrSystem.Infrustructure.Persistence;

public static class AppDbContextSeed
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly Dictionary<string, string> RoleAliasMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Admin"] = RoleNames.OrganizationAdmin,
        ["OrganizationAdmin"] = RoleNames.OrganizationAdmin,
        ["Organization Admin"] = RoleNames.OrganizationAdmin,
        ["HRManager"] = RoleNames.HRManager,
        ["HR Manager"] = RoleNames.HRManager,
        ["HRSpecialist"] = RoleNames.HRSpecialist,
        ["HR Specialist"] = RoleNames.HRSpecialist,
        ["IT Manager"] = RoleNames.DepartmentManager,
        ["Finance Manager"] = RoleNames.DepartmentManager,
        ["Operations Manager"] = RoleNames.DepartmentManager,
        ["Department Manager"] = RoleNames.DepartmentManager,
        ["Dept Manager"] = RoleNames.DepartmentManager,
        ["Employee"] = RoleNames.Employee
    };

    public static async Task SeedAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IWebHostEnvironment env)
    {
        var seedDataPath = Path.Combine(env.WebRootPath, "SeedData");

        if (!Directory.Exists(seedDataPath))
        {
            Console.WriteLine($"SeedData directory not found at: {seedDataPath}");
            return;
        }

        try
        {
            // Seed organization foundation before any dependent entities
            await SeedSubscriptionPlansAsync(context, seedDataPath);
            await SeedOrganizationAsync(context, seedDataPath);
            if (!await context.Organizations.AnyAsync())
            {
                Console.WriteLine("Organization seeding failed or organization already missing; aborting remaining seed steps.");
                return;
            }

            await SeedRolesAsync(roleManager, seedDataPath);
            await SeedCountriesAsync(context, seedDataPath);
            await SeedBranchesAsync(context, seedDataPath);
            if (!await context.Branches.AnyAsync())
            {
                Console.WriteLine("Branch seeding failed; aborting remaining seed steps.");
                return;
            }

            await SeedDepartmentsAsync(context, seedDataPath);
            await SeedJobTitlesAsync(context, seedDataPath);
            await SeedContractTypesAsync(context, seedDataPath);
            await SeedGendersAsync(context, seedDataPath);
            await SeedMaritalStatusesAsync(context, seedDataPath);
            await SeedEmployeeStatusesAsync(context, seedDataPath);
            await SeedAttendanceStatusesAsync(context, seedDataPath);

            if (await HasEmployeeSeedPrerequisitesAsync(context))
            {
                await SeedEmployeesAsync(context, userManager, roleManager, seedDataPath);
            }
            else
            {
                Console.WriteLine("Skipping employee seeding because prerequisite reference data is missing.");
            }

            await SeedRoleUsersAsync(context, userManager, roleManager, seedDataPath);
            await SeedEmployeeDocumentsAsync(context);
            await SeedLeaveStatusesAsync(context, seedDataPath);
            await SeedLeaveTypesAsync(context, seedDataPath);
            await SeedLeavePoliciesAsync(context, seedDataPath);
            await SeedLeaveBalancesAndHistoryAsync(context);
            await SeedAllowanceTypesAsync(context, seedDataPath);
            await SeedDeductionTypesAsync(context, seedDataPath);
            await SeedSocialInsuranceRatesAsync(context, seedDataPath);
            await SeedTaxBracketsAsync(context, seedDataPath);
            await SeedPublicHolidaysAsync(context, seedDataPath);
            await SeedWorkSchedulesAsync(context, seedDataPath);
            await SeedPayrollStatusesAsync(context, seedDataPath);
            await SeedEmployeeSalaryAndPayrollHistoryAsync(context);
            await SeedAttendanceHistoryAsync(context);
            await SeedEmployeeAssetsAsync(context);
            await SeedReviewTypesAsync(context, seedDataPath);
            await SeedReviewStatusesAsync(context, seedDataPath);
            await SeedGoalStatusesAsync(context, seedDataPath);
            await SeedGoalPrioritiesAsync(context, seedDataPath);
            await SeedOvertimeStatusesAsync(context, seedDataPath);
            await SeedInvoiceStatusesAsync(context, seedDataPath);
            await EnsureDefaultScopeForSeedData(context);

            Console.WriteLine("Database seeding completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during seeding: {ex.Message}");
            throw;
        }
    }

    private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager, string seedDataPath)
    {
        var filePath = Path.Combine(seedDataPath, "Roles.json");
        if (!File.Exists(filePath)) return;

        var json = await File.ReadAllTextAsync(filePath);
        var roles = JsonSerializer.Deserialize<List<RoleSeedData>>(json, _jsonOptions);

        if (roles == null) return;

        foreach (var roleData in roles)
        {
            var roleExists = await roleManager.RoleExistsAsync(roleData.Name);
            if (!roleExists)
            {
                var role = new ApplicationRole
                {
                    Name = roleData.Name,
                    DisplayName = roleData.DisplayName,
                    NormalizedName = roleData.NormalizedName
                };

                await roleManager.CreateAsync(role);
                Console.WriteLine($"Created role: {roleData.Name}");
            }
        }
    }

    private static async Task SeedSubscriptionPlansAsync(ApplicationDbContext context, string seedDataPath)
    {
        var filePath = Path.Combine(seedDataPath, "SubscriptionPlans.json");
        if (!File.Exists(filePath)) return;

        var json = await File.ReadAllTextAsync(filePath);
        var plans = JsonSerializer.Deserialize<List<SubscriptionPlanSeedData>>(json, _jsonOptions);

        if (plans == null || plans.Count == 0) return;

        var existingCodes = new HashSet<string>(
            await context.SubscriptionPlans
                .IgnoreQueryFilters()
                .Select(p => p.Code)
                .ToListAsync(),
            StringComparer.OrdinalIgnoreCase);

        var addedCount = 0;

        foreach (var planData in plans)
        {
            if (string.IsNullOrWhiteSpace(planData.Code) || existingCodes.Contains(planData.Code))
            {
                continue;
            }

            var plan = new SubscriptionPlan
            {
                Id = Guid.NewGuid(),
                Code = planData.Code,
                NameAr = planData.NameAr,
                NameEn = planData.NameEn,
                DescriptionAr = planData.DescriptionAr,
                DescriptionEn = planData.DescriptionEn,
                MonthlyPrice = planData.MonthlyPrice,
                AnnualPrice = planData.AnnualPrice,
                Currency = planData.Currency,
                MaxEmployees = planData.MaxEmployees,
                MaxStorageGB = planData.MaxStorageGB,
                MaxDepartments = planData.MaxDepartments,
                AllowBiometricIntegration = planData.AllowBiometricIntegration,
                AllowPayrollModule = planData.AllowPayrollModule,
                AllowPerformanceModule = planData.AllowPerformanceModule,
                AllowRecruitmentModule = planData.AllowRecruitmentModule,
                AllowCustomReports = planData.AllowCustomReports,
                AllowAPIAccess = planData.AllowAPIAccess,
                TrialDays = planData.TrialDays,
                IsActive = planData.IsActive,
                DisplayOrder = planData.DisplayOrder,
                CreatedDate = DateTimeOffset.UtcNow
            };

            await context.SubscriptionPlans.AddAsync(plan);
            existingCodes.Add(plan.Code);
            addedCount++;
        }

        if (addedCount > 0)
        {
            await context.SaveChangesAsync();
            Console.WriteLine($"Seeded {addedCount} subscription plans");
        }
    }

    private static async Task SeedOrganizationAsync(ApplicationDbContext context, string seedDataPath)
    {
        if (await context.Organizations.AnyAsync()) return;

        var filePath = Path.Combine(seedDataPath, "DemoOrganization.json");
        if (!File.Exists(filePath)) return;

        var json = await File.ReadAllTextAsync(filePath);
        var orgData = JsonSerializer.Deserialize<OrganizationSeedData>(json, _jsonOptions);

        if (orgData == null) return;

        var plan = await context.SubscriptionPlans.FirstOrDefaultAsync(p => p.Code == orgData.SubscriptionPlanCode);
        if (plan == null) return;

        var organizationId = Guid.NewGuid();

        var organization = new Organization
        {
            Id = organizationId,
            Code = orgData.Code,
            NameAr = orgData.NameAr,
            NameEn = orgData.NameEn,
            LogoUrl = orgData.LogoUrl,
            CommercialRegistrationNumber = orgData.CommercialRegistrationNumber,
            TaxRegistrationNumber = orgData.TaxRegistrationNumber,
            LegalEntityType = orgData.LegalEntityType,
            Email = orgData.Email,
            PhoneNumber = orgData.PhoneNumber,
            Website = orgData.Website,
            AddressAr = orgData.AddressAr,
            AddressEn = orgData.AddressEn,
            City = orgData.City,
            Country = orgData.Country,
            PostalCode = orgData.PostalCode,
            SubscriptionPlanId = plan.Id,
            SubscriptionStartDate = DateTime.UtcNow,
            SubscriptionEndDate = DateTime.UtcNow.AddDays(orgData.TrialDays),
            IsActive = orgData.IsActive,
            IsTrialPeriod = orgData.IsTrialPeriod,
            TrialEndDate = DateTime.UtcNow.AddDays(orgData.TrialDays),
            MaxEmployees = orgData.MaxEmployees,
            CurrentEmployeeCount = orgData.CurrentEmployeeCount,
            MaxStorageGB = orgData.MaxStorageGB,
            CurrentStorageGB = orgData.CurrentStorageGB,
            TimeZone = orgData.TimeZone,
            Currency = orgData.Currency,
            WeekStartDay = orgData.WeekStartDay,
            CreatedDate = DateTimeOffset.UtcNow,
            TenantId = organizationId
        };

        await context.Organizations.AddAsync(organization);
        await context.SaveChangesAsync();
        Console.WriteLine($"Seeded demo organization: {orgData.NameEn}");
    }

    private static async Task SeedRoleUsersAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        string seedDataPath)
    {
        var organization = await context.Organizations.FirstOrDefaultAsync();
        if (organization == null) return;

        var branches = await context.Branches.ToListAsync();
        var defaultBranch = branches.FirstOrDefault(b => b.IsHeadquarter) ?? branches.FirstOrDefault();
        var defaultBranchId = defaultBranch?.Id;

        var filePath = Path.Combine(seedDataPath, "Roles.json");
        if (!File.Exists(filePath)) return;

        var json = await File.ReadAllTextAsync(filePath);
        var roles = JsonSerializer.Deserialize<List<RoleSeedData>>(json, _jsonOptions);

        if (roles == null || roles.Count == 0) return;

        // Get lookup data for employee creation
        var departments = await context.Departments.ToListAsync();
        var jobTitles = await context.JobTitles.ToListAsync();

        // Roles that should have associated employee records
        var rolesRequiringEmployee = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            RoleNames.HRManager,
            RoleNames.HRSpecialist,
            RoleNames.DepartmentManager
        };

        var branchRoleAdded = false;
        var employeeCodeCounter = 900; // Start with a high number to avoid conflicts

        foreach (var roleData in roles)
        {
            var canonicalRoleName = NormalizeRoleName(roleData.Name) ?? roleData.Name;
            var roleExists = await roleManager.RoleExistsAsync(roleData.Name);
            if (!roleExists)
            {
                continue;
            }

            var usersInRole = await userManager.GetUsersInRoleAsync(roleData.Name);
            var hasUserInOrg = usersInRole.Any(u => u.OrganizationId == organization.Id);
            if (hasUserInOrg)
            {
                continue;
            }

            var emailPrefix = BuildRoleEmailPrefix(canonicalRoleName);
            var email = $"{emailPrefix}@{organization.Code.ToLowerInvariant()}.local";
            var existingUser = await userManager.FindByEmailAsync(email);

            var requiresBranchScope = RoleNames.RequiresBranchScope(canonicalRoleName);
            var branchIdForRole = requiresBranchScope ? defaultBranchId : null;

            // Create employee record for specific roles
            Guid? employeeId = null;
            if (rolesRequiringEmployee.Contains(canonicalRoleName) && defaultBranchId.HasValue)
            {
                employeeId = await CreateRoleEmployeeAsync(
                    context,
                    canonicalRoleName,
                    roleData.DisplayName,
                    email,
                    organization.Id,
                    defaultBranchId.Value,
                    departments,
                    jobTitles,
                    employeeCodeCounter++);
            }

            var user = existingUser ?? new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = GetRoleUserFullName(canonicalRoleName),
                IsActive = true,
                OrganizationId = organization.Id,
                EmployeeId = employeeId,
                CreatedDate = DateTimeOffset.UtcNow,
                BranchId = branchIdForRole ?? defaultBranchId
            };

            if (existingUser == null)
            {
                var createResult = await userManager.CreateAsync(user, "Password@123");
                if (!createResult.Succeeded)
                {
                    Console.WriteLine($"Failed to create user for role {roleData.Name}: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                    continue;
                }
            }
            else
            {
                var requiresUpdate = false;

                if (requiresBranchScope && branchIdForRole.HasValue && user.BranchId != branchIdForRole)
                {
                    user.BranchId = branchIdForRole;
                    requiresUpdate = true;
                }

                if (employeeId.HasValue && user.EmployeeId != employeeId)
                {
                    user.EmployeeId = employeeId;
                    requiresUpdate = true;
                }

                if (requiresUpdate)
                {
                    await userManager.UpdateAsync(user);
                }
            }

            // Link employee to user if created
            if (employeeId.HasValue)
            {
                var employee = await context.Employees.FindAsync(employeeId.Value);
                if (employee != null)
                {
                    employee.UserId = user.Id;
                    await context.SaveChangesAsync();
                }
            }

            var inRole = await userManager.IsInRoleAsync(user, roleData.Name);
            if (!inRole)
            {
                await userManager.AddToRoleAsync(user, roleData.Name);
            }

            if (defaultBranch != null)
            {
                var assigned = await EnsureUserBranchRoleAsync(context, user.Id, defaultBranch.Id, canonicalRoleName);
                branchRoleAdded = branchRoleAdded || assigned;
            }

            Console.WriteLine($"Ensured role user for: {canonicalRoleName}{(employeeId.HasValue ? " (with employee record)" : "")}");
        }

        if (branchRoleAdded)
        {
            await context.SaveChangesAsync();
        }
    }

    private static string GetRoleUserFullName(string roleName)
    {
        return roleName switch
        {
            RoleNames.HRManager => "Sarah Johnson",
            RoleNames.HRSpecialist => "Omar Ahmed",
            RoleNames.DepartmentManager => "Mohamed Ali",
            _ => roleName
        };
    }

    private static async Task<Guid?> CreateRoleEmployeeAsync(
        ApplicationDbContext context,
        string roleName,
        string displayName,
        string email,
        Guid tenantId,
        Guid branchId,
        List<Department> departments,
        List<JobTitle> jobTitles,
        int employeeCodeCounter)
    {
        // Find appropriate department and job title based on role
        var (departmentName, jobTitleName, firstNameEn, lastNameEn, firstNameAr, lastNameAr) = roleName switch
        {
            RoleNames.HRManager => ("Human Resources", "HR Manager", "Sarah", "Johnson", "سارة", "جونسون"),
            RoleNames.HRSpecialist => ("Human Resources", "HR Specialist", "Omar", "Ahmed", "عمر", "أحمد"),
            RoleNames.DepartmentManager => ("Operations", "Operations Manager", "Mohamed", "Ali", "محمد", "علي"),
            _ => (null as string, null as string, "System", "User", "مستخدم", "النظام")
        };

        var department = departments.FirstOrDefault(d =>
            d.NameEn.Contains(departmentName ?? "Human", StringComparison.OrdinalIgnoreCase))
            ?? departments.FirstOrDefault();

        var jobTitle = jobTitles.FirstOrDefault(j =>
            j.TitleEn.Contains(jobTitleName ?? "Manager", StringComparison.OrdinalIgnoreCase))
            ?? jobTitles.FirstOrDefault();

        if (department == null || jobTitle == null)
        {
            Console.WriteLine($"Cannot create employee for {roleName}: missing department or job title");
            return null;
        }

        // Use well-known GUIDs from seed data
        var maleGenderId = Guid.Parse("00000000-0000-0000-0006-000000000001");
        var femaleGenderId = Guid.Parse("00000000-0000-0000-0006-000000000002");
        var singleMaritalStatusId = Guid.Parse("00000000-0000-0000-0007-000000000001");
        var marriedMaritalStatusId = Guid.Parse("00000000-0000-0000-0007-000000000002");
        var permanentContractTypeId = Guid.Parse("00000000-0000-0000-0005-000000000001");
        var activeStatusId = Guid.Parse("00000000-0000-0000-0008-000000000001");

        var genderId = roleName == RoleNames.HRManager ? femaleGenderId : maleGenderId;
        var maritalStatusId = roleName == RoleNames.HRManager ? singleMaritalStatusId : marriedMaritalStatusId;

        var employeeId = Guid.NewGuid();
        var employee = new Employee
        {
            Id = employeeId,
            EmployeeCode = $"EMP-{employeeCodeCounter:D4}",
            FirstNameAr = firstNameAr,
            LastNameAr = lastNameAr,
            FirstNameEn = firstNameEn,
            LastNameEn = lastNameEn,
            NationalId = $"ROLE{employeeCodeCounter:D10}",
            DateOfBirth = DateTime.UtcNow.AddYears(-35),
            GenderId = genderId,
            MaritalStatusId = maritalStatusId,
            Email = email,
            PhoneNumber = $"+2012345{employeeCodeCounter:D4}",
            MobileNumber = $"+2010123{employeeCodeCounter:D5}",
            AddressAr = "القاهرة، مصر",
            AddressEn = "Cairo, Egypt",
            City = "Cairo",
            Country = "Egypt",
            DepartmentId = department.Id,
            JobTitleId = jobTitle.Id,
            BranchId = branchId,
            ContractTypeId = permanentContractTypeId,
            StatusId = activeStatusId,
            HiringDate = DateTime.UtcNow.AddYears(-3),
            ProbationPeriodMonths = 0,
            TenantId = tenantId,
            CreatedDate = DateTimeOffset.UtcNow
        };

        await context.Employees.AddAsync(employee);
        await context.SaveChangesAsync();

        Console.WriteLine($"Created employee {employee.EmployeeCode} for role {roleName}");
        return employeeId;
    }

    private static async Task SeedBranchesAsync(ApplicationDbContext context, string seedDataPath)
    {
        if (await context.Branches.AnyAsync()) return;

        var filePath = Path.Combine(seedDataPath, "Branches.json");
        if (!File.Exists(filePath)) return;

        var json = await File.ReadAllTextAsync(filePath);
        var branches = JsonSerializer.Deserialize<List<BranchSeedData>>(json, _jsonOptions);

        if (branches == null) return;

        var organization = await context.Organizations.FirstOrDefaultAsync();
        if (organization == null) return;

        foreach (var branchData in branches)
        {
            var branch = new Branch
            {
                Id = Guid.NewGuid(),
                OrganizationId = organization.Id,
                NameAr = branchData.NameAr,
                NameEn = branchData.NameEn,
                Code = branchData.Code,
                Description = branchData.Description,
                CountryId = Guid.Parse(branchData.CountryId),
                City = branchData.City,
                AddressAr = branchData.AddressAr,
                AddressEn = branchData.AddressEn,
                PostalCode = branchData.PostalCode,
                PhoneNumber = branchData.PhoneNumber,
                Email = branchData.Email,
                TimeZone = branchData.TimeZone,
                Currency = branchData.Currency,
                Language = branchData.Language,
                IsHeadquarter = branchData.IsHeadquarter,
                IsActive = branchData.IsActive,
                MaxEmployeeCapacity = branchData.MaxEmployeeCapacity,
                CurrentEmployeeCount = 0,
                OpeningDate = DateTime.UtcNow,
                WorkStartTime = TimeSpan.Parse(branchData.WorkStartTime),
                WorkEndTime = TimeSpan.Parse(branchData.WorkEndTime),
                WorkingDays = branchData.WorkingDays,
                TenantId = organization.Id,
                CreatedDate = DateTimeOffset.UtcNow
            };

            branch.BranchId = branch.Id;

            await context.Branches.AddAsync(branch);
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"Seeded {branches.Count} branches");
    }

    private static async Task SeedDepartmentsAsync(ApplicationDbContext context, string seedDataPath)
    {
        if (await context.Departments.AnyAsync()) return;

        var filePath = Path.Combine(seedDataPath, "Departments.json");
        if (!File.Exists(filePath)) return;

        var json = await File.ReadAllTextAsync(filePath);
        var departments = JsonSerializer.Deserialize<List<DepartmentSeedData>>(json, _jsonOptions);

        if (departments == null) return;

        var organization = await context.Organizations.FirstOrDefaultAsync();
        if (organization == null) return;

        var branches = await context.Branches.ToListAsync();
        var headquarter = branches.FirstOrDefault(b => b.IsHeadquarter);

        foreach (var deptData in departments)
        {
            var department = new Department
            {
                Id = Guid.NewGuid(),
                NameAr = deptData.NameAr,
                NameEn = deptData.NameEn,
                Description = deptData.Description,
                BranchId = headquarter?.Id, // Assign to headquarter by default
                TenantId = organization.Id,
                CreatedDate = DateTimeOffset.UtcNow
            };

            await context.Departments.AddAsync(department);
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"Seeded {departments.Count} departments");
    }

    private static async Task SeedJobTitlesAsync(ApplicationDbContext context, string seedDataPath)
    {
        if (await context.JobTitles.AnyAsync()) return;

        var filePath = Path.Combine(seedDataPath, "JobTitles.json");
        if (!File.Exists(filePath)) return;

        var json = await File.ReadAllTextAsync(filePath);
        var jobTitles = JsonSerializer.Deserialize<List<JobTitleSeedData>>(json, _jsonOptions);

        if (jobTitles == null) return;

        var organization = await context.Organizations.FirstOrDefaultAsync();
        if (organization == null) return;

        var defaultBranchId = await GetDefaultBranchIdAsync(context);
        if (!defaultBranchId.HasValue) return;

        foreach (var titleData in jobTitles)
        {
            var jobTitle = new JobTitle
            {
                Id = Guid.NewGuid(),
                TitleAr = titleData.TitleAr,
                TitleEn = titleData.TitleEn,
                Description = titleData.Description,
                Level = titleData.Level,
                MinSalary = titleData.MinSalary,
                MaxSalary = titleData.MaxSalary,
                TenantId = organization.Id,
                BranchId = defaultBranchId.Value,
                CreatedDate = DateTimeOffset.UtcNow
            };

            await context.JobTitles.AddAsync(jobTitle);
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"Seeded {jobTitles.Count} job titles");
    }

    private static async Task SeedEmployeesAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, string seedDataPath)
    {
        if (await context.Employees.AnyAsync()) return;

        var filePath = Path.Combine(seedDataPath, "Employees.json");
        if (!File.Exists(filePath)) return;

        var json = await File.ReadAllTextAsync(filePath);
        var employees = JsonSerializer.Deserialize<List<EmployeeSeedData>>(json, _jsonOptions);

        if (employees == null) return;

        var organization = await context.Organizations.FirstOrDefaultAsync();
        if (organization == null) return;

        var departments = await context.Departments.ToListAsync();
        var jobTitles = await context.JobTitles.ToListAsync();
        var branches = await context.Branches.ToListAsync();
        var departmentLookup = await BuildDepartmentLookupAsync(departments, seedDataPath);
        var jobTitleLookup = await BuildJobTitleLookupAsync(jobTitles, seedDataPath);

        Department? ResolveDepartment(string? code)
        {
            if (!string.IsNullOrWhiteSpace(code) && departmentLookup.TryGetValue(code, out var mappedDepartment))
            {
                return mappedDepartment;
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                return null;
            }

            return departments.FirstOrDefault(d => d.NameEn.StartsWith(code, StringComparison.OrdinalIgnoreCase));
        }

        JobTitle? ResolveJobTitle(string? code)
        {
            if (!string.IsNullOrWhiteSpace(code) && jobTitleLookup.TryGetValue(code, out var mappedJobTitle))
            {
                return mappedJobTitle;
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                return null;
            }

            var normalizedCode = NormalizeCodeKey(code);
            return jobTitles.FirstOrDefault(j =>
                j.TitleEn.Contains(code, StringComparison.OrdinalIgnoreCase) ||
                NormalizeCodeKey(j.TitleEn).Contains(normalizedCode, StringComparison.OrdinalIgnoreCase));
        }

        // Dictionary to store employee codes and their IDs for manager assignment
        var employeeMap = new Dictionary<string, Guid>();

        // First pass: Create all employees without manager assignment
        foreach (var empData in employees)
        {
            var department = ResolveDepartment(empData.DepartmentCode);
            var jobTitle = ResolveJobTitle(empData.JobTitleCode);
            var branch = branches.FirstOrDefault(b => string.Equals(b.Code, empData.BranchCode, StringComparison.OrdinalIgnoreCase));

            if (department == null || jobTitle == null || branch == null)
            {
                Console.WriteLine($"Skipping employee {empData.EmployeeCode}: department={department?.NameEn}, jobTitle={jobTitle?.TitleEn}, branch={branch?.Code}");
                continue;
            }

            var employeeId = Guid.NewGuid();
            var employee = new Employee
            {
                Id = employeeId,
                EmployeeCode = empData.EmployeeCode,
                FirstNameAr = empData.FirstNameAr,
                LastNameAr = empData.LastNameAr,
                FirstNameEn = empData.FirstNameEn,
                LastNameEn = empData.LastNameEn,
                NationalId = empData.NationalId,
                PassportNumber = empData.PassportNumber,
                DateOfBirth = DateTime.Parse(empData.DateOfBirth),
                GenderId = Guid.Parse(empData.GenderId),
                MaritalStatusId = Guid.Parse(empData.MaritalStatusId),
                Email = empData.Email,
                PhoneNumber = empData.PhoneNumber,
                MobileNumber = empData.MobileNumber,
                AddressAr = empData.AddressAr,
                AddressEn = empData.AddressEn,
                City = empData.City,
                Country = empData.Country,
                DepartmentId = department.Id,
                JobTitleId = jobTitle.Id,
                BranchId = branch.Id,
                ContractTypeId = Guid.Parse(empData.ContractTypeId),
                StatusId = Guid.Parse(empData.StatusId),
                HiringDate = DateTime.Parse(empData.HiringDate),
                ProbationPeriodMonths = empData.ProbationPeriodMonths,
                TenantId = organization.Id,
                CreatedDate = DateTimeOffset.UtcNow
            };

            employeeMap[empData.EmployeeCode] = employeeId;
            await context.Employees.AddAsync(employee);
            
            // Save the employee first to satisfy FK constraint before creating user
            await context.SaveChangesAsync();

            // Create user account for employee
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = empData.Email,
                Email = empData.Email,
                EmailConfirmed = true,
                FullName = $"{empData.FirstNameEn} {empData.LastNameEn}",
                IsActive = true,
                OrganizationId = organization.Id,
                EmployeeId = employeeId,
                BranchId = branch.Id,
                CreatedDate = DateTimeOffset.UtcNow
            };

            var result = await userManager.CreateAsync(user, "Password@123");
            if (!result.Succeeded)
            {
                Console.WriteLine($"Failed to create user for employee {empData.EmployeeCode}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
            else if (!string.IsNullOrWhiteSpace(empData.Role))
            {
                var normalizedRole = NormalizeRoleName(empData.Role);
                if (!string.IsNullOrEmpty(normalizedRole))
                {
                    var roleExists = await roleManager.RoleExistsAsync(normalizedRole);
                    if (roleExists)
                    {
                        var assignResult = await userManager.AddToRoleAsync(user, normalizedRole);
                        if (!assignResult.Succeeded)
                        {
                            Console.WriteLine($"Failed to assign role {normalizedRole} to {empData.Email}: {string.Join(", ", assignResult.Errors.Select(e => e.Description))}");
                        }
                        else
                        {
                            await EnsureUserBranchRoleAsync(context, user.Id, branch.Id, normalizedRole);
                            Console.WriteLine($"Created user: {empData.Email} with role: {normalizedRole}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Role {normalizedRole} not found for employee {empData.EmployeeCode}");
                    }
                }
                else
                {
                    Console.WriteLine($"Unknown role '{empData.Role}' for employee {empData.EmployeeCode}; skipping assignment");
                }
            }

            employee.UserId = user.Id;
        }

        await context.SaveChangesAsync();

        // Second pass: Update manager assignments
        var allEmployees = await context.Employees.ToListAsync();
        foreach (var empData in employees.Where(e => !string.IsNullOrEmpty(e.DirectManagerCode)))
        {
            var managerCode = empData.DirectManagerCode!;
            if (employeeMap.TryGetValue(managerCode, out var managerId))
            {
                var employee = allEmployees.FirstOrDefault(e => e.EmployeeCode == empData.EmployeeCode);
                if (employee != null)
                {
                    employee.DirectManagerId = managerId;
                }
            }
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"Seeded {employees.Count} employees");
    }

    private static async Task<Dictionary<string, Department>> BuildDepartmentLookupAsync(
        List<Department> departments,
        string seedDataPath)
    {
        var lookup = new Dictionary<string, Department>(StringComparer.OrdinalIgnoreCase);
        if (departments.Count == 0)
        {
            return lookup;
        }

        var filePath = Path.Combine(seedDataPath, "Departments.json");
        if (!File.Exists(filePath))
        {
            return lookup;
        }

        var json = await File.ReadAllTextAsync(filePath);
        var seeds = JsonSerializer.Deserialize<List<DepartmentSeedData>>(json, _jsonOptions);
        if (seeds == null)
        {
            return lookup;
        }

        foreach (var seed in seeds)
        {
            if (string.IsNullOrWhiteSpace(seed.Code) || string.IsNullOrWhiteSpace(seed.NameEn))
            {
                continue;
            }

            var department = departments.FirstOrDefault(d =>
                string.Equals(d.NameEn, seed.NameEn, StringComparison.OrdinalIgnoreCase));

            if (department == null)
            {
                continue;
            }

            lookup.TryAdd(seed.Code, department);

            var normalizedCode = NormalizeCodeKey(seed.Code);
            if (!string.IsNullOrEmpty(normalizedCode))
            {
                lookup.TryAdd(normalizedCode, department);
            }
        }

        return lookup;
    }

    private static async Task<Dictionary<string, JobTitle>> BuildJobTitleLookupAsync(
        List<JobTitle> jobTitles,
        string seedDataPath)
    {
        var lookup = new Dictionary<string, JobTitle>(StringComparer.OrdinalIgnoreCase);
        if (jobTitles.Count == 0)
        {
            return lookup;
        }

        var filePath = Path.Combine(seedDataPath, "JobTitles.json");
        if (!File.Exists(filePath))
        {
            return lookup;
        }

        var json = await File.ReadAllTextAsync(filePath);
        var seeds = JsonSerializer.Deserialize<List<JobTitleSeedData>>(json, _jsonOptions);
        if (seeds == null)
        {
            return lookup;
        }

        foreach (var seed in seeds)
        {
            if (string.IsNullOrWhiteSpace(seed.Code) || string.IsNullOrWhiteSpace(seed.TitleEn))
            {
                continue;
            }

            var jobTitle = jobTitles.FirstOrDefault(j =>
                string.Equals(j.TitleEn, seed.TitleEn, StringComparison.OrdinalIgnoreCase));

            if (jobTitle == null)
            {
                continue;
            }

            lookup.TryAdd(seed.Code, jobTitle);

            var normalizedCode = NormalizeCodeKey(seed.Code);
            if (!string.IsNullOrEmpty(normalizedCode))
            {
                lookup.TryAdd(normalizedCode, jobTitle);
            }
        }

        return lookup;
    }

    private static string NormalizeCodeKey(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return string.Concat(value
            .Where(c => !char.IsWhiteSpace(c) && c != '-' && c != '_')
            .Select(char.ToUpperInvariant));
    }

    private static async Task SeedLeaveStatusesAsync(ApplicationDbContext context, string seedDataPath)
    {
        if (await context.LeaveStatuses.AnyAsync()) return;

        var filePath = Path.Combine(seedDataPath, "LeaveStatuses.json");
        if (!File.Exists(filePath)) return;

        var json = await File.ReadAllTextAsync(filePath);
        var statuses = JsonSerializer.Deserialize<List<LeaveStatusSeedData>>(json, _jsonOptions);

        if (statuses == null) return;

        var organization = await context.Organizations.FirstOrDefaultAsync();
        if (organization == null) return;

        var defaultBranchId = await GetDefaultBranchIdAsync(context);
        if (!defaultBranchId.HasValue) return;

        foreach (var statusData in statuses)
        {
            var status = new HrSystem.Domain.Entities.Leave.LeaveStatus
            {
                Id = statusData.Id,
                NameEn = statusData.NameEn,
                NameAr = statusData.NameAr,
                Description = statusData.Description,
                DisplayOrder = statusData.DisplayOrder,
                TenantId = organization.Id,
                BranchId = defaultBranchId.Value,
                CreatedDate = DateTimeOffset.UtcNow
            };

            await context.LeaveStatuses.AddAsync(status);
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"Seeded {statuses.Count} leave statuses");
    }

    private static async Task SeedLeaveTypesAsync(ApplicationDbContext context, string seedDataPath)
    {
        if (await context.LeaveTypes.AnyAsync()) return;

        var filePath = Path.Combine(seedDataPath, "LeaveTypes.json");
        if (!File.Exists(filePath)) return;

        var json = await File.ReadAllTextAsync(filePath);
        var types = JsonSerializer.Deserialize<List<LeaveTypeSeedData>>(json, _jsonOptions);

        if (types == null) return;

        var organization = await context.Organizations.FirstOrDefaultAsync();
        if (organization == null) return;

        var defaultBranchId = await GetDefaultBranchIdAsync(context);
        if (!defaultBranchId.HasValue) return;

        foreach (var typeData in types)
        {
            var leaveType = new HrSystem.Domain.Entities.Leave.LeaveType
            {
                Id = typeData.Id,
                NameEn = typeData.NameEn,
                NameAr = typeData.NameAr,
                Description = typeData.Description,
                Icon = typeData.Icon,
                ColorCode = typeData.ColorCode,
                DisplayOrder = typeData.DisplayOrder,
                IsActive = typeData.IsActive,
                TenantId = organization.Id,
                BranchId = defaultBranchId.Value,
                CreatedDate = DateTimeOffset.UtcNow
            };

            await context.LeaveTypes.AddAsync(leaveType);
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"Seeded {types.Count} leave types");
    }

    private static async Task SeedLeavePoliciesAsync(ApplicationDbContext context, string seedDataPath)
    {
        if (await context.LeavePolicies.AnyAsync()) return;

        var filePath = Path.Combine(seedDataPath, "LeavePolicies.json");
        if (!File.Exists(filePath)) return;

        var json = await File.ReadAllTextAsync(filePath);
        var policies = JsonSerializer.Deserialize<List<LeavePolicySeedData>>(json, _jsonOptions);

        if (policies == null) return;

        var organization = await context.Organizations.FirstOrDefaultAsync();
        if (organization == null) return;

        var defaultBranchId = await GetDefaultBranchIdAsync(context);
        if (!defaultBranchId.HasValue) return;

        foreach (var policyData in policies)
        {
            var policy = new LeavePolicy
            {
                Id = Guid.NewGuid(),
                LeaveTypeId = policyData.LeaveTypeId,
                NameAr = policyData.NameAr,
                NameEn = policyData.NameEn,
                DefaultDaysPerYear = policyData.DefaultDaysPerYear,
                MaxCarryForward = policyData.MaxCarryForward,
                RequiresApproval = policyData.RequiresApproval,
                RequiresManagerApproval = policyData.RequiresManagerApproval,
                RequiresHRApproval = policyData.RequiresHRApproval,
                IsPaid = policyData.IsPaid,
                MaxConsecutiveDays = policyData.MaxConsecutiveDays,
                MinDaysNotice = policyData.MinDaysNotice,
                RequiresDocument = policyData.RequiresDocument,
                Description = policyData.Description,
                TenantId = organization.Id,
                BranchId = defaultBranchId.Value,
                CreatedDate = DateTimeOffset.UtcNow
            };

            await context.LeavePolicies.AddAsync(policy);
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"Seeded {policies.Count} leave policies");
    }

    private static async Task SeedAllowanceTypesAsync(ApplicationDbContext context, string seedDataPath)
    {
        if (await context.AllowanceTypes.AnyAsync()) return;

        var filePath = Path.Combine(seedDataPath, "AllowanceTypes.json");
        if (!File.Exists(filePath)) return;

        var json = await File.ReadAllTextAsync(filePath);
        var allowances = JsonSerializer.Deserialize<List<AllowanceTypeSeedData>>(json, _jsonOptions);

        if (allowances == null) return;

        var organization = await context.Organizations.FirstOrDefaultAsync();
        if (organization == null) return;

        var defaultBranchId = await GetDefaultBranchIdAsync(context);
        if (!defaultBranchId.HasValue) return;

        foreach (var allowanceData in allowances)
        {
            var allowance = new AllowanceType
            {
                Id = Guid.NewGuid(),
                NameAr = allowanceData.NameAr,
                NameEn = allowanceData.NameEn,
                Description = allowanceData.Description,
                IsTaxable = allowanceData.IsTaxable,
                IsSubjectToInsurance = allowanceData.IsSubjectToInsurance,
                TenantId = organization.Id,
                BranchId = defaultBranchId.Value,
                CreatedDate = DateTimeOffset.UtcNow
            };

            await context.AllowanceTypes.AddAsync(allowance);
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"Seeded {allowances.Count} allowance types");
    }

    private static async Task SeedDeductionTypesAsync(ApplicationDbContext context, string seedDataPath)
    {
        if (await context.DeductionTypes.AnyAsync()) return;

        var filePath = Path.Combine(seedDataPath, "DeductionTypes.json");
        if (!File.Exists(filePath)) return;

        var json = await File.ReadAllTextAsync(filePath);
        var deductions = JsonSerializer.Deserialize<List<DeductionTypeSeedData>>(json, _jsonOptions);

        if (deductions == null) return;

        var organization = await context.Organizations.FirstOrDefaultAsync();
        if (organization == null) return;

        var defaultBranchId = await GetDefaultBranchIdAsync(context);
        if (!defaultBranchId.HasValue) return;

        foreach (var deductionData in deductions)
        {
            var deduction = new DeductionType
            {
                Id = Guid.NewGuid(),
                NameAr = deductionData.NameAr,
                NameEn = deductionData.NameEn,
                Description = deductionData.Description,
                IsRecurring = deductionData.IsRecurring,
                TenantId = organization.Id,
                BranchId = defaultBranchId.Value,
                CreatedDate = DateTimeOffset.UtcNow
            };

            await context.DeductionTypes.AddAsync(deduction);
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"Seeded {deductions.Count} deduction types");
    }

    private static async Task SeedEmployeeDocumentsAsync(ApplicationDbContext context)
    {
        if (await context.EmployeeDocuments.AnyAsync())
        {
            return;
        }

        var employees = await context.Employees
            .AsNoTracking()
            .Select(e => new EmployeeDocumentSeedInfo(
                e.Id,
                e.TenantId,
                e.BranchId,
                e.EmployeeCode,
                e.FirstNameEn,
                e.LastNameEn,
                e.HiringDate))
            .ToListAsync();

        if (employees.Count == 0)
        {
            return;
        }

        var defaultBranchId = await GetDefaultBranchIdAsync(context);
        if (!defaultBranchId.HasValue)
        {
            return;
        }

        var templates = BuildDocumentTemplates();

        if (templates.Count == 0)
        {
            Console.WriteLine("No matching employee document templates found; skipping employee document seeding.");
            return;
        }

        var random = new Random(20260130);
        var documents = new List<EmployeeDocument>(employees.Count * 3);

        foreach (var employee in employees)
        {
            var minPerEmployee = Math.Min(2, templates.Count);
            var maxPerEmployee = templates.Count;
            var templateCount = maxPerEmployee == minPerEmployee
                ? minPerEmployee
                : random.Next(minPerEmployee, maxPerEmployee + 1);

            var selectedTemplates = templates
                .OrderBy(_ => random.Next())
                .Take(Math.Max(1, templateCount))
                .ToList();

            foreach (var template in selectedTemplates)
            {
                var folder = BuildDocumentFolder(employee.EmployeeCode, employee.Id);
                var uniqueSuffix = employee.Id.ToString("N")[..6];
                var filePath = $"seed/documents/{folder}/{template.FileSlug}-{uniqueSuffix}.pdf";

                var baseDate = employee.HiringDate ?? DateTime.UtcNow;
                var expiry = template.HasExpiry
                    ? baseDate.AddYears(template.ExpiryOffsetYears)
                    : (DateTime?)null;

                var minSize = Math.Max(10_000, template.MinFileSize);
                var maxSize = Math.Max(minSize, template.MaxFileSize);
                var fileSize = minSize == maxSize
                    ? minSize
                    : random.NextInt64(minSize, maxSize + 1);

                documents.Add(new EmployeeDocument
                {
                    EmployeeId = employee.Id,
                    DocumentType = template.DocumentType,
                    DocumentName = $"{template.DisplayName} - {employee.FirstName?.Trim()} {employee.LastName?.Trim()}".Trim(),
                    FilePath = filePath,
                    FileUrl = $"https://cdn.demo-hrsystem.local/{filePath}",
                    Description = template.Description,
                    ExpiryDate = expiry,
                    FileSize = fileSize,
                    ContentType = template.ContentType,
                    TenantId = employee.TenantId,
                    BranchId = employee.BranchId ?? defaultBranchId.Value,
                    CreatedDate = DateTimeOffset.UtcNow
                });
            }
        }

        if (documents.Count > 0)
        {
            await context.EmployeeDocuments.AddRangeAsync(documents);
            await context.SaveChangesAsync();
            Console.WriteLine($"Seeded {documents.Count} employee documents");
        }
    }

    private static List<DocumentTemplate> BuildDocumentTemplates()
    {
        return new List<DocumentTemplate>
        {
            new(
                EmployeeDocumentType.NationalId,
                "National ID Copy",
                "national-id",
                "application/pdf",
                "Scanned national identification card.",
                true,
                10,
                150_000,
                350_000),
            new(
                EmployeeDocumentType.EmploymentContract,
                "Signed Employment Contract",
                "employment-contract",
                "application/pdf",
                "Signed employment contract including compensation terms.",
                false,
                0,
                500_000,
                900_000),
            new(
                EmployeeDocumentType.CV,
                "Curriculum Vitae",
                "curriculum-vitae",
                "application/pdf",
                "Latest CV submitted by the employee.",
                false,
                0,
                250_000,
                450_000),
            new(
                EmployeeDocumentType.Certificates,
                "Professional Certificates",
                "certificates",
                "application/pdf",
                "Supporting certifications or diplomas.",
                false,
                0,
                300_000,
                650_000),
            new(
                EmployeeDocumentType.MedicalReport,
                "Medical Report",
                "medical-report",
                "application/pdf",
                "Latest medical clearance report.",
                true,
                2,
                250_000,
                500_000)
        };
    }

    private static string BuildDocumentFolder(string? employeeCode, Guid employeeId)
    {
        var baseSegment = string.IsNullOrWhiteSpace(employeeCode)
            ? employeeId.ToString("N")[..8]
            : employeeCode.Trim();

        var sanitized = new string(baseSegment
            .Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_')
            .ToArray());

        if (string.IsNullOrEmpty(sanitized))
        {
            sanitized = employeeId.ToString("N")[..8];
        }

        return sanitized.ToUpperInvariant();
    }

    private static async Task SeedSocialInsuranceRatesAsync(ApplicationDbContext context, string seedDataPath)
    {
        if (await context.SocialInsuranceRates.AnyAsync()) return;

        var filePath = Path.Combine(seedDataPath, "SocialInsuranceRates.json");
        if (!File.Exists(filePath)) return;

        var json = await File.ReadAllTextAsync(filePath);
        var rates = JsonSerializer.Deserialize<List<SocialInsuranceRateSeedData>>(json, _jsonOptions);

        if (rates == null) return;

        var organization = await context.Organizations.FirstOrDefaultAsync();
        if (organization == null) return;

        var defaultBranchId = await GetDefaultBranchIdAsync(context);
        if (!defaultBranchId.HasValue) return;

        foreach (var rateData in rates)
        {
            var rate = new SocialInsuranceRate
            {
                Id = Guid.NewGuid(),
                Year = rateData.Year,
                EmployeeRate = rateData.EmployeeRate,
                EmployerRate = rateData.EmployerRate,
                MinSalaryBase = rateData.MinSalaryBase,
                MaxSalaryBase = rateData.MaxSalaryBase,
                IsActive = rateData.IsActive,
                Description = rateData.Description,
                TenantId = organization.Id,
                BranchId = defaultBranchId.Value,
                CreatedDate = DateTimeOffset.UtcNow
            };

            await context.SocialInsuranceRates.AddAsync(rate);
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"Seeded {rates.Count} social insurance rates");
    }

    private static async Task SeedTaxBracketsAsync(ApplicationDbContext context, string seedDataPath)
    {
        if (await context.TaxBrackets.AnyAsync()) return;

        var filePath = Path.Combine(seedDataPath, "TaxBrackets.json");
        if (!File.Exists(filePath)) return;

        var json = await File.ReadAllTextAsync(filePath);
        var brackets = JsonSerializer.Deserialize<List<TaxBracketSeedData>>(json, _jsonOptions);

        if (brackets == null) return;

        var organization = await context.Organizations.FirstOrDefaultAsync();
        if (organization == null) return;

        var defaultBranchId = await GetDefaultBranchIdAsync(context);
        if (!defaultBranchId.HasValue) return;

        foreach (var bracketData in brackets)
        {
            var bracket = new TaxBracket
            {
                Id = Guid.NewGuid(),
                Year = bracketData.Year,
                MinIncome = bracketData.MinIncome,
                MaxIncome = bracketData.MaxIncome,
                TaxRate = bracketData.TaxRate,
                FixedAmount = bracketData.FixedAmount,
                IsActive = bracketData.IsActive,
                Description = bracketData.Description,
                TenantId = organization.Id,
                BranchId = defaultBranchId.Value,
                CreatedDate = DateTimeOffset.UtcNow
            };

            await context.TaxBrackets.AddAsync(bracket);
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"Seeded {brackets.Count} tax brackets");
    }

    private static async Task SeedPublicHolidaysAsync(ApplicationDbContext context, string seedDataPath)
    {
        if (await context.PublicHolidays.AnyAsync()) return;

        var filePath = Path.Combine(seedDataPath, "PublicHolidays.json");
        if (!File.Exists(filePath)) return;

        var json = await File.ReadAllTextAsync(filePath);
        var holidays = JsonSerializer.Deserialize<List<PublicHolidaySeedData>>(json, _jsonOptions);

        if (holidays == null) return;

        var organization = await context.Organizations.FirstOrDefaultAsync();
        if (organization == null) return;

        var defaultBranchId = await GetDefaultBranchIdAsync(context);
        if (!defaultBranchId.HasValue) return;

        foreach (var holidayData in holidays)
        {
            var holiday = new PublicHoliday
            {
                Id = Guid.NewGuid(),
                NameAr = holidayData.NameAr,
                NameEn = holidayData.NameEn,
                Date = DateTime.Parse(holidayData.Date),
                Year = holidayData.Year,
                IsRecurring = holidayData.IsRecurring,
                Description = holidayData.Description,
                TenantId = organization.Id,
                BranchId = defaultBranchId.Value,
                CreatedDate = DateTimeOffset.UtcNow
            };

            await context.PublicHolidays.AddAsync(holiday);
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"Seeded {holidays.Count} public holidays");
    }

    private static async Task SeedWorkSchedulesAsync(ApplicationDbContext context, string seedDataPath)
    {
        if (await context.WorkSchedules.AnyAsync()) return;

        var filePath = Path.Combine(seedDataPath, "WorkSchedules.json");
        if (!File.Exists(filePath)) return;

        var json = await File.ReadAllTextAsync(filePath);
        var schedules = JsonSerializer.Deserialize<List<WorkScheduleSeedData>>(json, _jsonOptions);

        if (schedules == null) return;

        var organization = await context.Organizations.FirstOrDefaultAsync();
        if (organization == null) return;

        var defaultBranchId = await GetDefaultBranchIdAsync(context);
        if (!defaultBranchId.HasValue) return;

        foreach (var scheduleData in schedules)
        {
            var schedule = new WorkSchedule
            {
                Id = Guid.NewGuid(),
                Name = scheduleData.Name,
                StartTime = TimeSpan.Parse(scheduleData.StartTime),
                EndTime = TimeSpan.Parse(scheduleData.EndTime),
                BreakDuration = string.IsNullOrEmpty(scheduleData.BreakDuration) ? null : TimeSpan.Parse(scheduleData.BreakDuration),
                WorkingHoursPerDay = scheduleData.WorkingHoursPerDay,
                WorkingDaysPerWeek = scheduleData.WorkingDaysPerWeek,
                GracePeriodLate = string.IsNullOrEmpty(scheduleData.GracePeriodLate) ? null : TimeSpan.Parse(scheduleData.GracePeriodLate),
                GracePeriodEarlyLeave = string.IsNullOrEmpty(scheduleData.GracePeriodEarlyLeave) ? null : TimeSpan.Parse(scheduleData.GracePeriodEarlyLeave),
                IsSaturday = scheduleData.IsSaturday,
                IsSunday = scheduleData.IsSunday,
                IsMonday = scheduleData.IsMonday,
                IsTuesday = scheduleData.IsTuesday,
                IsWednesday = scheduleData.IsWednesday,
                IsThursday = scheduleData.IsThursday,
                IsFriday = scheduleData.IsFriday,
                IsDefault = scheduleData.IsDefault,
                TenantId = organization.Id,
                BranchId = defaultBranchId.Value,
                CreatedDate = DateTimeOffset.UtcNow
            };

            await context.WorkSchedules.AddAsync(schedule);
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"Seeded {schedules.Count} work schedules");
    }

    private static string? NormalizeRoleName(string? roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            return null;
        }

        if (RoleAliasMap.TryGetValue(roleName.Trim(), out var mappedRole))
        {
            return mappedRole;
        }

        var condensed = new string(roleName
            .Where(char.IsLetterOrDigit)
            .ToArray());

        if (string.IsNullOrEmpty(condensed))
        {
            return null;
        }

        return RoleAliasMap.TryGetValue(condensed, out var alias)
            ? alias
            : condensed;
    }

    private static string BuildRoleEmailPrefix(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            return "roleuser";
        }

        var normalized = NormalizeCodeKey(roleName);
        if (!string.IsNullOrEmpty(normalized))
        {
            return normalized.ToLowerInvariant();
        }

        var filtered = new string(roleName
            .Where(char.IsLetterOrDigit)
            .ToArray());

        return string.IsNullOrEmpty(filtered)
            ? "roleuser"
            : filtered.ToLowerInvariant();
    }

    private static async Task<bool> EnsureUserBranchRoleAsync(ApplicationDbContext context, Guid userId, Guid branchId, string roleName)
    {
        var exists = await context.UserBranchRoles
            .AnyAsync(ubr => ubr.UserId == userId && ubr.BranchId == branchId && ubr.RoleName == roleName);
        if (exists)
        {
            return false;
        }

        await context.UserBranchRoles.AddAsync(new UserBranchRole
        {
            UserId = userId,
            BranchId = branchId,
            RoleName = roleName
        });

        return true;
    }

    private static async Task EnsureDefaultScopeForSeedData(ApplicationDbContext context)
    {
        var organization = await context.Organizations.FirstOrDefaultAsync();
        if (organization == null)
        {
            return;
        }

        var defaultBranch = await context.Branches
            .OrderByDescending(b => b.IsHeadquarter)
            .ThenBy(b => b.CreatedDate)
            .FirstOrDefaultAsync()
            ?? await context.Branches.FirstOrDefaultAsync();

        if (defaultBranch == null)
        {
            return;
        }

        var scopeUpdated = false;

        var setMethod = typeof(DbContext).GetMethod(nameof(DbContext.Set), new[] { typeof(Type) });
        if (setMethod == null)
        {
            return;
        }

        foreach (var entityType in context.Model.GetEntityTypes())
        {
            if (!typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            var set = setMethod.Invoke(context, new object[] { entityType.ClrType }) as IQueryable;
            if (set == null)
            {
                continue;
            }

            var needsScope = await set.Cast<BaseEntity>()
                .Where(e => e.TenantId == Guid.Empty || !e.BranchId.HasValue || e.BranchId == Guid.Empty)
                .ToListAsync();

            if (needsScope.Count == 0)
            {
                continue;
            }

            foreach (var entity in needsScope)
            {
                if (entity.TenantId == Guid.Empty)
                {
                    entity.TenantId = organization.Id;
                }

                if (!entity.BranchId.HasValue || entity.BranchId == Guid.Empty)
                {
                    entity.BranchId = defaultBranch.Id;
                }
            }

            scopeUpdated = true;
        }

        if (scopeUpdated)
        {
            await context.SaveChangesAsync();
        }
    }

    private static async Task<Guid?> GetDefaultBranchIdAsync(ApplicationDbContext context)
    {
        var branch = await context.Branches
            .OrderByDescending(b => b.IsHeadquarter)
            .ThenBy(b => b.CreatedDate)
            .FirstOrDefaultAsync()
            ?? await context.Branches.FirstOrDefaultAsync();

        return branch?.Id;
    }

    private sealed record EmployeeSeedInfo(Guid Id, Guid TenantId, Guid? BranchId, Guid? DirectManagerId, DateTime? HiringDate);
    private sealed record EmployeeDocumentSeedInfo(
        Guid Id,
        Guid TenantId,
        Guid? BranchId,
        string? EmployeeCode,
        string? FirstName,
        string? LastName,
        DateTime? HiringDate);

    private sealed record DocumentTemplate(
        EmployeeDocumentType DocumentType,
        string DisplayName,
        string FileSlug,
        string ContentType,
        string Description,
        bool HasExpiry,
        int ExpiryOffsetYears,
        long MinFileSize,
        long MaxFileSize);

    private static async Task SeedLeaveBalancesAndHistoryAsync(ApplicationDbContext context)
    {
        if (await context.LeaveBalances.AnyAsync())
        {
            return;
        }

        var employees = await context.Employees
            .AsNoTracking()
            .Select(e => new EmployeeSeedInfo(
                e.Id,
                e.TenantId,
                e.BranchId,
                e.DirectManagerId,
                e.HiringDate))
            .ToListAsync();

        if (employees.Count == 0)
        {
            return;
        }

        var annualPolicy = await context.LeavePolicies
            .AsNoTracking()
            .OrderBy(lp => lp.NameEn)
            .FirstOrDefaultAsync(lp => lp.NameEn == "Annual Leave")
            ?? await context.LeavePolicies.AsNoTracking().FirstOrDefaultAsync();

        if (annualPolicy == null)
        {
            return;
        }

        if (annualPolicy.DefaultDaysPerYear <= 0)
        {
            Console.WriteLine("Annual leave policy has no entitlement configured; skipping leave balance seeding.");
            return;
        }

        var leaveStatuses = await context.LeaveStatuses
            .AsNoTracking()
            .OrderBy(ls => ls.DisplayOrder)
            .ToListAsync();

        if (leaveStatuses.Count == 0)
        {
            return;
        }

        var approvedStatusId = leaveStatuses
            .FirstOrDefault(s => s.NameEn.Equals("Approved", StringComparison.OrdinalIgnoreCase))?.Id
            ?? leaveStatuses.First().Id;

        var defaultBranchId = await GetDefaultBranchIdAsync(context);
        if (!defaultBranchId.HasValue)
        {
            return;
        }

        var random = new Random(20260130);
        var currentYear = DateTime.UtcNow.Year;
        var previousYear = currentYear - 1;

        var leaveBalances = new List<LeaveBalance>();
        var leaveRequests = new List<LeaveRequest>();

        foreach (var employee in employees)
        {
            var prevTotal = annualPolicy.DefaultDaysPerYear;
            var prevUsedTarget = Math.Max(5, random.Next(8, Math.Max(9, prevTotal)));
            var prevUsed = Math.Min(Math.Max(prevTotal - 1, 0), prevUsedTarget);
            var prevRemaining = Math.Max(0, prevTotal - prevUsed);

            leaveBalances.Add(new LeaveBalance
            {
                EmployeeId = employee.Id,
                LeavePolicyId = annualPolicy.Id,
                Year = previousYear,
                TotalDays = prevTotal,
                UsedDays = prevUsed,
                RemainingDays = prevRemaining,
                CarriedForwardDays = 0,
                TenantId = employee.TenantId,
                BranchId = employee.BranchId ?? defaultBranchId.Value,
                CreatedDate = DateTimeOffset.UtcNow
            });

            var carryForward = Math.Min(prevRemaining, annualPolicy.MaxCarryForward);
            var currentTotal = annualPolicy.DefaultDaysPerYear + carryForward;
            var currentUsedTarget = Math.Max(4, random.Next(6, Math.Max(7, currentTotal)));
            var currentUsed = Math.Min(Math.Max(currentTotal - 1, 0), currentUsedTarget);
            var currentRemaining = Math.Max(0, currentTotal - currentUsed);

            leaveBalances.Add(new LeaveBalance
            {
                EmployeeId = employee.Id,
                LeavePolicyId = annualPolicy.Id,
                Year = currentYear,
                TotalDays = currentTotal,
                UsedDays = currentUsed,
                RemainingDays = currentRemaining,
                CarriedForwardDays = carryForward,
                TenantId = employee.TenantId,
                BranchId = employee.BranchId ?? defaultBranchId.Value,
                CreatedDate = DateTimeOffset.UtcNow
            });

            CreateLeaveRequestsForYear(employee, annualPolicy, approvedStatusId, previousYear, prevUsed, leaveRequests, random, defaultBranchId.Value);
            CreateLeaveRequestsForYear(employee, annualPolicy, approvedStatusId, currentYear, currentUsed, leaveRequests, random, defaultBranchId.Value);
        }

        if (leaveBalances.Count > 0)
        {
            await context.LeaveBalances.AddRangeAsync(leaveBalances);
        }

        if (leaveRequests.Count > 0)
        {
            await context.LeaveRequests.AddRangeAsync(leaveRequests);
        }

        if (leaveBalances.Count > 0 || leaveRequests.Count > 0)
        {
            await context.SaveChangesAsync();
            Console.WriteLine($"Seeded {leaveBalances.Count} leave balances and {leaveRequests.Count} leave requests");
        }
    }

    private static void CreateLeaveRequestsForYear(
        EmployeeSeedInfo employee,
        LeavePolicy policy,
        Guid approvedStatusId,
        int year,
        int usedDays,
        List<LeaveRequest> requests,
        Random random,
        Guid fallbackBranchId)
    {
        if (usedDays <= 0)
        {
            return;
        }

        var yearStart = new DateTime(year, 1, 1);
        var yearEnd = new DateTime(year, 12, 31);

        // Skip if employee was hired after the year ends
        if (employee.HiringDate.HasValue && employee.HiringDate.Value.Date > yearEnd)
        {
            return;
        }

        var earliestStart = employee.HiringDate.HasValue && employee.HiringDate.Value.Date > yearStart
            ? employee.HiringDate.Value.Date
            : yearStart;

        if (earliestStart > yearEnd)
        {
            return;
        }

        var currentStartCursor = earliestStart;

        // Guarantee at least 2 requests per year if usedDays allows
        var requestCount = Math.Max(2, Math.Min(4, usedDays / 3 + 1));
        var daysRemaining = usedDays;
        var requestsCreated = 0;

        for (var i = 0; i < requestCount && daysRemaining > 0; i++)
        {
            if (currentStartCursor > yearEnd)
            {
                break;
            }

            // Calculate available window
            var availableDays = (yearEnd - currentStartCursor).Days + 1;
            if (availableDays <= 0)
            {
                break;
            }

            // Size the chunk: at least 1 day, at most 5 days or what remains
            var maxChunk = Math.Min(5, Math.Min(daysRemaining, availableDays));
            var chunk = Math.Max(1, random.Next(1, maxChunk + 1));

            var startWindow = availableDays - chunk;
            var startOffset = startWindow > 0 ? random.Next(0, Math.Min(startWindow, 30)) : 0;
            var startDate = currentStartCursor.AddDays(startOffset);
            var endDate = startDate.AddDays(chunk - 1);

            // Clamp end date to year end
            if (endDate > yearEnd)
            {
                endDate = yearEnd;
                chunk = (endDate - startDate).Days + 1;
            }

            if (chunk <= 0)
            {
                break;
            }

            requests.Add(new LeaveRequest
            {
                EmployeeId = employee.Id,
                LeavePolicyId = policy.Id,
                LeaveTypeId = policy.LeaveTypeId,
                StartDate = startDate,
                EndDate = endDate,
                TotalDays = chunk,
                Reason = $"Auto-generated annual leave ({year})",
                LeaveStatusId = approvedStatusId,
                ManagerId = employee.DirectManagerId,
                ManagerApprovalDate = startDate.AddDays(-2),
                ManagerComments = "Approved automatically",
                HRApprovedBy = employee.DirectManagerId,
                HRApprovalDate = startDate.AddDays(-1),
                HRComments = "Seeded record",
                TenantId = employee.TenantId,
                BranchId = employee.BranchId ?? fallbackBranchId,
                CreatedDate = DateTimeOffset.UtcNow
            });

            daysRemaining -= chunk;
            requestsCreated++;
            currentStartCursor = endDate.AddDays(random.Next(7, 30));
        }

        // Fallback: if no requests created but usedDays > 0, force one at start of eligible window
        if (requestsCreated == 0 && usedDays > 0)
        {
            var availableDays = (yearEnd - earliestStart).Days + 1;
            var chunk = Math.Min(usedDays, Math.Min(5, availableDays));
            if (chunk > 0)
            {
                var startDate = earliestStart;
                var endDate = startDate.AddDays(chunk - 1);

                requests.Add(new LeaveRequest
                {
                    EmployeeId = employee.Id,
                    LeavePolicyId = policy.Id,
                    LeaveTypeId = policy.LeaveTypeId,
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalDays = chunk,
                    Reason = $"Auto-generated annual leave ({year})",
                    LeaveStatusId = approvedStatusId,
                    ManagerId = employee.DirectManagerId,
                    ManagerApprovalDate = startDate.AddDays(-2),
                    ManagerComments = "Approved automatically",
                    HRApprovedBy = employee.DirectManagerId,
                    HRApprovalDate = startDate.AddDays(-1),
                    HRComments = "Seeded record",
                    TenantId = employee.TenantId,
                    BranchId = employee.BranchId ?? fallbackBranchId,
                    CreatedDate = DateTimeOffset.UtcNow
                });
            }
        }
    }

    private static async Task SeedEmployeeSalaryAndPayrollHistoryAsync(ApplicationDbContext context)
    {
        var employees = await context.Employees
            .Include(e => e.JobTitle)
            .AsNoTracking()
            .ToListAsync();

        if (employees.Count == 0)
        {
            return;
        }

        var organization = await context.Organizations.AsNoTracking().FirstOrDefaultAsync();
        if (organization == null)
        {
            return;
        }

        var defaultBranchId = await GetDefaultBranchIdAsync(context);
        if (!defaultBranchId.HasValue)
        {
            return;
        }

        var random = new Random(20260130);

        var employeesWithSalary = new HashSet<Guid>(
            await context.Salaries
                .AsNoTracking()
                .Select(s => s.EmployeeId)
                .Distinct()
                .ToListAsync());

        var newSalaries = new List<Salary>();
        foreach (var employee in employees)
        {
            if (employeesWithSalary.Contains(employee.Id))
            {
                continue;
            }

            var minSalary = employee.JobTitle?.MinSalary > 0 ? employee.JobTitle.MinSalary : 15000m;
            var maxSalary = employee.JobTitle?.MaxSalary > minSalary ? employee.JobTitle.MaxSalary : minSalary + 5000m;
            var baseSalary = GenerateSalaryAmount(random, minSalary, maxSalary);

            newSalaries.Add(new Salary
            {
                EmployeeId = employee.Id,
                BasicSalary = baseSalary,
                EffectiveDate = employee.HiringDate ?? DateTime.UtcNow,
                Notes = "Seeded base salary",
                IsCurrent = true,
                TenantId = employee.TenantId,
                BranchId = employee.BranchId ?? defaultBranchId.Value,
                CreatedDate = DateTimeOffset.UtcNow
            });
        }

        if (newSalaries.Count > 0)
        {
            await context.Salaries.AddRangeAsync(newSalaries);
            await context.SaveChangesAsync();
            Console.WriteLine($"Seeded {newSalaries.Count} salary records");
        }

        if (await context.PayrollCycles.AnyAsync())
        {
            return;
        }

        var currentSalaries = await context.Salaries
            .Where(s => s.IsCurrent)
            .AsNoTracking()
            .ToDictionaryAsync(s => s.EmployeeId);

        if (currentSalaries.Count == 0)
        {
            return;
        }

        var payrollStatuses = await context.PayrollStatuses
            .AsNoTracking()
            .ToListAsync();

        if (payrollStatuses.Count == 0)
        {
            return;
        }

        var payrollStatusId =
            payrollStatuses.FirstOrDefault(s => s.NameEn.Equals("Paid", StringComparison.OrdinalIgnoreCase))?.Id
            ?? payrollStatuses.FirstOrDefault(s => s.NameEn.Equals("Processed", StringComparison.OrdinalIgnoreCase))?.Id
            ?? payrollStatuses.First().Id;

        var payrollCycles = new List<PayrollCycle>();
        var today = DateTime.UtcNow;

        for (var offset = 0; offset < 3; offset++)
        {
            var periodStart = new DateTime(today.Year, today.Month, 1).AddMonths(-offset);
            var periodEnd = periodStart.AddMonths(1).AddDays(-1);

            var cycle = new PayrollCycle
            {
                CycleName = periodStart.ToString("MMMM yyyy", CultureInfo.InvariantCulture),
                Month = periodStart.Month,
                Year = periodStart.Year,
                PeriodStartDate = periodStart,
                PeriodEndDate = periodEnd,
                PaymentDate = periodEnd.AddDays(3),
                StatusId = payrollStatusId,
                Notes = "Auto-generated sample payroll cycle",
                TenantId = organization.Id,
                BranchId = defaultBranchId.Value,
                CreatedDate = DateTimeOffset.UtcNow
            };

            decimal totalGross = 0;
            decimal totalNet = 0;
            decimal totalDeductions = 0;
            decimal totalTax = 0;
            decimal totalInsurance = 0;

            foreach (var salary in currentSalaries.Values)
            {
                var allowances = Math.Round(salary.BasicSalary * (decimal)(0.08 + random.NextDouble() * 0.05), 2, MidpointRounding.AwayFromZero);
                var overtime = Math.Round(salary.BasicSalary * (decimal)(0.01 + random.NextDouble() * 0.02), 2, MidpointRounding.AwayFromZero);
                var bonus = random.NextDouble() < 0.3
                    ? Math.Round(salary.BasicSalary * (decimal)(0.03 + random.NextDouble() * 0.04), 2, MidpointRounding.AwayFromZero)
                    : 0m;

                var gross = salary.BasicSalary + allowances + overtime + bonus;
                var tax = Math.Round(gross * 0.05m, 2, MidpointRounding.AwayFromZero);
                var insuranceEmployee = Math.Round(gross * 0.08m, 2, MidpointRounding.AwayFromZero);
                var insuranceEmployer = Math.Round(gross * 0.10m, 2, MidpointRounding.AwayFromZero);
                var unpaidLeaveDays = random.NextDouble() < 0.15 ? random.Next(0, 3) : 0;
                var leaveDeductions = Math.Round((salary.BasicSalary / 22m) * unpaidLeaveDays, 2, MidpointRounding.AwayFromZero);
                var totalDeduction = tax + insuranceEmployee + leaveDeductions;
                var netSalary = gross - totalDeduction;

                var payslip = new Payslip
                {
                    PayrollCycleId = cycle.Id,
                    EmployeeId = salary.EmployeeId,
                    PayslipNumber = $"PS-{periodStart:yyyyMM}-{salary.EmployeeId.ToString("N")[..6].ToUpperInvariant()}",
                    BasicSalary = salary.BasicSalary,
                    TotalAllowances = allowances + overtime + bonus,
                    GrossSalary = gross,
                    TotalDeductions = totalDeduction,
                    IncomeTax = tax,
                    SocialInsuranceEmployee = insuranceEmployee,
                    SocialInsuranceEmployer = insuranceEmployer,
                    OvertimeAmount = overtime,
                    BonusAmount = bonus,
                    LeaveDeductions = leaveDeductions,
                    UnpaidLeaveDays = unpaidLeaveDays,
                    NetSalary = netSalary,
                    TotalWorkingDays = 22,
                    ActualWorkingDays = 22 - unpaidLeaveDays,
                    AbsentDays = unpaidLeaveDays,
                    GeneratedDate = DateTime.UtcNow,
                    IsPaid = true,
                    PaidDate = cycle.PaymentDate,
                    TenantId = salary.TenantId,
                    BranchId = salary.BranchId ?? defaultBranchId.Value,
                    CreatedDate = DateTimeOffset.UtcNow
                };

                cycle.Payslips.Add(payslip);

                totalGross += gross;
                totalNet += netSalary;
                totalDeductions += totalDeduction;
                totalTax += tax;
                totalInsurance += insuranceEmployee + insuranceEmployer;
            }

            cycle.TotalGrossSalary = totalGross;
            cycle.TotalNetSalary = totalNet;
            cycle.TotalDeductions = totalDeductions;
            cycle.TotalTax = totalTax;
            cycle.TotalInsurance = totalInsurance;

            payrollCycles.Add(cycle);
        }

        if (payrollCycles.Count > 0)
        {
            await context.PayrollCycles.AddRangeAsync(payrollCycles);
            await context.SaveChangesAsync();
            Console.WriteLine($"Seeded {payrollCycles.Count} payroll cycles and {payrollCycles.Sum(c => c.Payslips.Count)} payslips");
        }
    }

    private static async Task SeedAttendanceHistoryAsync(ApplicationDbContext context)
    {
        if (await context.Attendances.AnyAsync())
        {
            return;
        }

        var employees = await context.Employees.AsNoTracking().ToListAsync();
        if (employees.Count == 0)
        {
            return;
        }

        var defaultBranchId = await GetDefaultBranchIdAsync(context);
        if (!defaultBranchId.HasValue)
        {
            return;
        }

        var statuses = await context.AttendanceStatuses.AsNoTracking().ToListAsync();
        if (statuses.Count == 0)
        {
            return;
        }

        var statusLookup = statuses.ToDictionary(s => s.NameEn, s => s.Id, StringComparer.OrdinalIgnoreCase);
        var presentStatusId = statusLookup.TryGetValue("Present", out var present) ? present : statuses.First().Id;
        var weekendStatusId = statusLookup.TryGetValue("Weekend", out var weekend) ? weekend : presentStatusId;
        var absentStatusId = statusLookup.TryGetValue("Absent", out var absent) ? absent : presentStatusId;
        var lateStatusId = statusLookup.TryGetValue("Late", out var late) ? late : presentStatusId;
        var leaveStatusId = statusLookup.TryGetValue("On Leave", out var leave) ? leave : presentStatusId;

        var random = new Random(20260130);
        var endDate = DateTime.UtcNow.Date.AddDays(-1);
        var startDate = endDate.AddDays(-30);
        var shiftStart = new TimeSpan(9, 0, 0);
        var shiftEnd = new TimeSpan(17, 0, 0);

        var attendanceRecords = new List<Attendance>();

        foreach (var employee in employees)
        {
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var isWeekend = date.DayOfWeek is DayOfWeek.Friday or DayOfWeek.Saturday;
                var statusId = presentStatusId;

                TimeSpan? checkIn = null;
                TimeSpan? checkOut = null;
                TimeSpan? worked = null;
                TimeSpan? overtime = null;
                TimeSpan? lateMinutes = null;
                TimeSpan? earlyLeave = null;
                var flaggedLate = false;
                var flaggedEarlyLeave = false;
                var flaggedOvertime = false;

                if (isWeekend)
                {
                    statusId = weekendStatusId;
                }
                else
                {
                    var roll = random.NextDouble();
                    if (roll < 0.08)
                    {
                        statusId = absentStatusId;
                    }
                    else if (roll < 0.12)
                    {
                        statusId = leaveStatusId;
                    }
                    else
                    {
                        var isLate = roll < 0.25;
                        statusId = isLate ? lateStatusId : presentStatusId;

                        var startOffset = isLate
                            ? random.Next(5, 40)
                            : random.Next(-10, 11);

                        checkIn = shiftStart.Add(TimeSpan.FromMinutes(startOffset));
                        checkOut = shiftEnd.Add(TimeSpan.FromMinutes(random.Next(-20, 60)));

                        var workedRaw = checkOut.Value - checkIn.Value;
                        worked = workedRaw < TimeSpan.Zero ? TimeSpan.Zero : workedRaw;

                        if (checkIn > shiftStart.Add(TimeSpan.FromMinutes(5)))
                        {
                            flaggedLate = true;
                            lateMinutes = checkIn - shiftStart;
                        }

                        if (checkOut < shiftEnd)
                        {
                            flaggedEarlyLeave = true;
                            earlyLeave = shiftEnd - checkOut;
                        }
                        else if (checkOut > shiftEnd.Add(TimeSpan.FromMinutes(15)))
                        {
                            flaggedOvertime = true;
                            overtime = checkOut - shiftEnd;
                        }
                    }
                }

                attendanceRecords.Add(new Attendance
                {
                    EmployeeId = employee.Id,
                    Date = date,
                    CheckInTime = checkIn,
                    CheckOutTime = checkOut,
                    StatusId = statusId,
                    DeviceId = "SEED-DEVICE",
                    CheckInDeviceId = checkIn.HasValue ? "SEED-DEVICE" : null,
                    CheckOutDeviceId = checkOut.HasValue ? "SEED-DEVICE" : null,
                    WorkedHours = worked,
                    OvertimeHours = overtime,
                    LateMinutes = lateMinutes,
                    EarlyLeaveMinutes = earlyLeave,
                    IsLate = flaggedLate,
                    IsEarlyLeave = flaggedEarlyLeave,
                    IsOvertime = flaggedOvertime,
                    Notes = "Auto-generated attendance record",
                    TenantId = employee.TenantId,
                    BranchId = employee.BranchId ?? defaultBranchId.Value,
                    CreatedDate = DateTimeOffset.UtcNow
                });
            }
        }

        if (attendanceRecords.Count > 0)
        {
            await context.Attendances.AddRangeAsync(attendanceRecords);
            await context.SaveChangesAsync();
            Console.WriteLine($"Seeded {attendanceRecords.Count} attendance records");
        }
    }

    private static async Task SeedEmployeeAssetsAsync(ApplicationDbContext context)
    {
        if (await context.EmployeeAssets.AnyAsync())
        {
            return;
        }

        var employees = await context.Employees.AsNoTracking().ToListAsync();
        if (employees.Count == 0)
        {
            return;
        }

        var defaultBranchId = await GetDefaultBranchIdAsync(context);
        if (!defaultBranchId.HasValue)
        {
            return;
        }

        var assetTemplates = new[]
        {
            new AssetTemplate("Laptop", "Dell Latitude 7440", "Latitude 7440", 28500m),
            new AssetTemplate("Laptop", "HP EliteBook 840", "EliteBook 840 G10", 26500m),
            new AssetTemplate("Mobile", "iPhone 15", "A3090", 36000m),
            new AssetTemplate("Mobile", "Samsung Galaxy S24", "SM-S921B", 32000m),
            new AssetTemplate("Access Card", "RFID Access Card", "AC-100", 150m),
            new AssetTemplate("Monitor", "Dell UltraSharp 27", "U2723QE", 14500m),
            new AssetTemplate("Headset", "Jabra Evolve2 65", "Evolve2 65", 3500m)
        };

        var random = new Random(20260130);
        var assets = new List<EmployeeAsset>();

        foreach (var employee in employees)
        {
            var assetCount = 1 + random.Next(0, 2);
            for (var i = 0; i < assetCount; i++)
            {
                var template = assetTemplates[random.Next(assetTemplates.Length)];
            var hiringDate = employee.HiringDate ?? DateTime.UtcNow;
            var assignedDate = hiringDate.AddDays(random.Next(0, 45));
                var serialPrefix = template.AssetType[..Math.Min(3, template.AssetType.Length)].ToUpperInvariant();
                var asset = new EmployeeAsset
                {
                    EmployeeId = employee.Id,
                    AssetType = template.AssetType,
                    AssetName = template.AssetName,
                    SerialNumber = $"{serialPrefix}-{random.Next(10000, 99999)}",
                    Model = template.Model,
                    Description = $"Issued {template.AssetType.ToLowerInvariant()} for daily operations",
                    AssignedDate = assignedDate,
                    ExpectedReturnDate = assignedDate.AddYears(1),
                    IsReturned = false,
                    Condition = "Good",
                    Value = template.Value,
                    TenantId = employee.TenantId,
                    BranchId = employee.BranchId ?? defaultBranchId.Value,
                    CreatedDate = DateTimeOffset.UtcNow
                };

                if (random.NextDouble() < 0.1)
                {
                    asset.IsReturned = true;
                    asset.ReturnDate = asset.AssignedDate.AddMonths(random.Next(6, 14));
                    asset.ReturnNotes = "Returned after upgrade";
                }

                assets.Add(asset);
            }
        }

        if (assets.Count > 0)
        {
            await context.EmployeeAssets.AddRangeAsync(assets);
            await context.SaveChangesAsync();
            Console.WriteLine($"Seeded {assets.Count} employee assets");
        }
    }

    private static decimal GenerateSalaryAmount(Random random, decimal minSalary, decimal maxSalary)
    {
        if (maxSalary <= minSalary)
        {
            maxSalary = minSalary + 1000m;
        }

        var sample = (double)minSalary + random.NextDouble() * (double)(maxSalary - minSalary);
        var rounded = Math.Round((decimal)sample / 50m, 0, MidpointRounding.AwayFromZero) * 50m;
        return rounded;
    }

    private sealed record AssetTemplate(string AssetType, string AssetName, string Model, decimal Value);

    // Seed Data DTOs

    private static async Task SeedGoalStatusesAsync(ApplicationDbContext context, string seedDataPath)
    {
        if (await context.GoalStatuses.AnyAsync()) return;

        var filePath = Path.Combine(seedDataPath, "GoalStatuses.json");
        if (!File.Exists(filePath)) return;

        var json = await File.ReadAllTextAsync(filePath);
        var statuses = JsonSerializer.Deserialize<List<StatusSeedData>>(json, _jsonOptions);

        if (statuses == null) return;

        var organization = await context.Organizations.FirstOrDefaultAsync();
        if (organization == null) return;

        var defaultBranchId = await GetDefaultBranchIdAsync(context);
        if (!defaultBranchId.HasValue) return;

        foreach (var statusData in statuses)
        {
            var status = new GoalStatus
            {
                Id = Guid.NewGuid(),
                Code = statusData.Code,
                NameAr = statusData.NameAr,
                NameEn = statusData.NameEn,
                DescriptionAr = statusData.DescriptionAr,
                DescriptionEn = statusData.DescriptionEn,
                ColorCode = statusData.ColorCode,
                DisplayOrder = statusData.DisplayOrder,
                IsActive = statusData.IsActive,
                TenantId = organization.Id,
                BranchId = defaultBranchId.Value,
                CreatedDate = DateTimeOffset.UtcNow
            };

            await context.GoalStatuses.AddAsync(status);
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"Seeded {statuses.Count} goal statuses");
    }

    private static async Task SeedGoalPrioritiesAsync(ApplicationDbContext context, string seedDataPath)
    {
        if (await context.GoalPriorities.AnyAsync()) return;

        var filePath = Path.Combine(seedDataPath, "GoalPriorities.json");
        if (!File.Exists(filePath)) return;

        var json = await File.ReadAllTextAsync(filePath);
        var priorities = JsonSerializer.Deserialize<List<StatusSeedData>>(json, _jsonOptions);

        if (priorities == null) return;

        var organization = await context.Organizations.FirstOrDefaultAsync();
        if (organization == null) return;

        var defaultBranchId = await GetDefaultBranchIdAsync(context);
        if (!defaultBranchId.HasValue) return;

        foreach (var priorityData in priorities)
        {
            var priority = new GoalPriority
            {
                Id = Guid.NewGuid(),
                Code = priorityData.Code,
                NameAr = priorityData.NameAr,
                NameEn = priorityData.NameEn,
                DescriptionAr = priorityData.DescriptionAr,
                DescriptionEn = priorityData.DescriptionEn,
                ColorCode = priorityData.ColorCode,
                DisplayOrder = priorityData.DisplayOrder,
                IsActive = priorityData.IsActive,
                TenantId = organization.Id,
                BranchId = defaultBranchId.Value,
                CreatedDate = DateTimeOffset.UtcNow
            };

            await context.GoalPriorities.AddAsync(priority);
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"Seeded {priorities.Count} goal priorities");
    }

    private static async Task SeedReviewTypesAsync(ApplicationDbContext context, string seedDataPath)
    {
        if (await context.ReviewTypes.AnyAsync()) return;

        var filePath = Path.Combine(seedDataPath, "ReviewTypes.json");
        if (!File.Exists(filePath)) return;

        var json = await File.ReadAllTextAsync(filePath);
        var reviewTypes = JsonSerializer.Deserialize<List<StatusSeedData>>(json, _jsonOptions);

        if (reviewTypes == null) return;

        var organization = await context.Organizations.FirstOrDefaultAsync();
        if (organization == null) return;

        var defaultBranchId = await GetDefaultBranchIdAsync(context);
        if (!defaultBranchId.HasValue) return;

        foreach (var typeData in reviewTypes)
        {
            var reviewType = new ReviewType
            {
                Id = typeData.Id,
                Code = typeData.Code,
                NameAr = typeData.NameAr,
                NameEn = typeData.NameEn,
                DescriptionAr = typeData.DescriptionAr,
                DescriptionEn = typeData.DescriptionEn,
                DisplayOrder = typeData.DisplayOrder,
                IsActive = typeData.IsActive,
                TenantId = organization.Id,
                BranchId = defaultBranchId.Value,
                CreatedDate = DateTimeOffset.UtcNow
            };

            await context.ReviewTypes.AddAsync(reviewType);
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"Seeded {reviewTypes.Count} review types");
    }

    private static async Task SeedReviewStatusesAsync(ApplicationDbContext context, string seedDataPath)
    {
        if (await context.ReviewStatuses.AnyAsync()) return;

        var filePath = Path.Combine(seedDataPath, "ReviewStatuses.json");
        if (!File.Exists(filePath)) return;

        var json = await File.ReadAllTextAsync(filePath);
        var reviewStatuses = JsonSerializer.Deserialize<List<StatusSeedData>>(json, _jsonOptions);

        if (reviewStatuses == null) return;

        var organization = await context.Organizations.FirstOrDefaultAsync();
        if (organization == null) return;

        var defaultBranchId = await GetDefaultBranchIdAsync(context);
        if (!defaultBranchId.HasValue) return;

        foreach (var statusData in reviewStatuses)
        {
            var reviewStatus = new ReviewStatus
            {
                Id = statusData.Id,
                Code = statusData.Code,
                NameAr = statusData.NameAr,
                NameEn = statusData.NameEn,
                DescriptionAr = statusData.DescriptionAr,
                DescriptionEn = statusData.DescriptionEn,
                ColorCode = statusData.ColorCode,
                DisplayOrder = statusData.DisplayOrder,
                IsActive = statusData.IsActive,
                TenantId = organization.Id,
                BranchId = defaultBranchId.Value,
                CreatedDate = DateTimeOffset.UtcNow
            };

            await context.ReviewStatuses.AddAsync(reviewStatus);
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"Seeded {reviewStatuses.Count} review statuses");
    }

    private static async Task SeedOvertimeStatusesAsync(ApplicationDbContext context, string seedDataPath)
    {
        if (await context.OvertimeStatuses.AnyAsync()) return;

        var filePath = Path.Combine(seedDataPath, "OvertimeStatuses.json");
        if (!File.Exists(filePath)) return;

        var json = await File.ReadAllTextAsync(filePath);
        var overtimeStatuses = JsonSerializer.Deserialize<List<StatusSeedData>>(json, _jsonOptions);

        if (overtimeStatuses == null) return;

        var organization = await context.Organizations.FirstOrDefaultAsync();
        if (organization == null) return;

        var defaultBranchId = await GetDefaultBranchIdAsync(context);
        if (!defaultBranchId.HasValue) return;

        foreach (var statusData in overtimeStatuses)
        {
            var overtimeStatus = new OvertimeStatus
            {
                Id = Guid.NewGuid(),
                Code = statusData.Code,
                NameAr = statusData.NameAr,
                NameEn = statusData.NameEn,
                DescriptionAr = statusData.DescriptionAr,
                DescriptionEn = statusData.DescriptionEn,
                ColorCode = statusData.ColorCode,
                DisplayOrder = statusData.DisplayOrder,
                IsActive = statusData.IsActive,
                TenantId = organization.Id,
                BranchId = defaultBranchId.Value,
                CreatedDate = DateTimeOffset.UtcNow
            };

            await context.OvertimeStatuses.AddAsync(overtimeStatus);
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"Seeded {overtimeStatuses.Count} overtime statuses");
    }

    private static async Task SeedInvoiceStatusesAsync(ApplicationDbContext context, string seedDataPath)
    {
        if (await context.InvoiceStatuses.AnyAsync()) return;

        var filePath = Path.Combine(seedDataPath, "InvoiceStatuses.json");
        if (!File.Exists(filePath)) return;

        var json = await File.ReadAllTextAsync(filePath);
        var invoiceStatuses = JsonSerializer.Deserialize<List<StatusSeedData>>(json, _jsonOptions);

        if (invoiceStatuses == null) return;

        var organization = await context.Organizations.FirstOrDefaultAsync();
        if (organization == null) return;

        var defaultBranchId = await GetDefaultBranchIdAsync(context);
        if (!defaultBranchId.HasValue) return;

        foreach (var statusData in invoiceStatuses)
        {
            var invoiceStatus = new InvoiceStatus
            {
                Id = Guid.NewGuid(),
                Code = statusData.Code,
                NameAr = statusData.NameAr,
                NameEn = statusData.NameEn,
                DescriptionAr = statusData.DescriptionAr,
                DescriptionEn = statusData.DescriptionEn,
                ColorCode = statusData.ColorCode,
                DisplayOrder = statusData.DisplayOrder,
                IsActive = statusData.IsActive,
                TenantId = organization.Id,
                BranchId = defaultBranchId.Value,
                CreatedDate = DateTimeOffset.UtcNow
            };

            await context.InvoiceStatuses.AddAsync(invoiceStatus);
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"Seeded {invoiceStatuses.Count} invoice statuses");
    }

    private static async Task<bool> HasEmployeeSeedPrerequisitesAsync(ApplicationDbContext context)
    {
        var hasDepartments = await context.Departments.AnyAsync();
        var hasJobTitles = await context.JobTitles.AnyAsync();
        var hasBranches = await context.Branches.AnyAsync();
        var hasContractTypes = await context.ContractTypes.AnyAsync();
        var hasGenders = await context.Genders.AnyAsync();
        var hasMaritalStatuses = await context.MaritalStatuses.AnyAsync();
        var hasEmployeeStatuses = await context.EmployeeStatuses.AnyAsync();

        return hasDepartments && hasJobTitles && hasBranches &&
               hasContractTypes && hasGenders && hasMaritalStatuses && hasEmployeeStatuses;
    }

    private static async Task SeedAttendanceStatusesAsync(ApplicationDbContext context, string seedDataPath)
    {
        if (await context.AttendanceStatuses.AnyAsync()) return;

        var filePath = Path.Combine(seedDataPath, "AttendanceStatuses.json");
        if (!File.Exists(filePath)) return;

        var json = await File.ReadAllTextAsync(filePath);
        var statuses = JsonSerializer.Deserialize<List<AttendanceStatusSeedData>>(json, _jsonOptions);

        if (statuses == null) return;

        var organization = await context.Organizations.FirstOrDefaultAsync();
        if (organization == null) return;

        var defaultBranchId = await GetDefaultBranchIdAsync(context);
        if (!defaultBranchId.HasValue) return;

        foreach (var statusData in statuses)
        {
            var status = new HrSystem.Domain.Entities.Attendance.AttendanceStatus
            {
                Id = statusData.Id,
                NameEn = statusData.NameEn,
                NameAr = statusData.NameAr,
                Description = statusData.Description,
                ColorCode = statusData.ColorCode,
                DisplayOrder = statusData.DisplayOrder,
                IsActive = statusData.IsActive,
                TenantId = organization.Id,
                BranchId = defaultBranchId.Value,
                CreatedDate = DateTimeOffset.UtcNow
            };

            await context.AttendanceStatuses.AddAsync(status);
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"Seeded {statuses.Count} attendance statuses");
    }

    private static async Task SeedContractTypesAsync(ApplicationDbContext context, string seedDataPath)
    {
        if (await context.ContractTypes.AnyAsync()) return;

        var filePath = Path.Combine(seedDataPath, "ContractTypes.json");
        if (!File.Exists(filePath)) return;

        var json = await File.ReadAllTextAsync(filePath);
        var types = JsonSerializer.Deserialize<List<ContractTypeSeedData>>(json, _jsonOptions);

        if (types == null) return;

        var organization = await context.Organizations.FirstOrDefaultAsync();
        if (organization == null) return;

        var defaultBranchId = await GetDefaultBranchIdAsync(context);
        if (!defaultBranchId.HasValue) return;

        foreach (var typeData in types)
        {
            var type = new HrSystem.Domain.Entities.Employee.ContractType
            {
                Id = typeData.Id,
                NameEn = typeData.NameEn,
                NameAr = typeData.NameAr,
                Description = typeData.Description,
                DisplayOrder = typeData.DisplayOrder,
                IsActive = typeData.IsActive,
                TenantId = organization.Id,
                BranchId = defaultBranchId.Value,
                CreatedDate = DateTimeOffset.UtcNow
            };

            await context.ContractTypes.AddAsync(type);
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"Seeded {types.Count} contract types");
    }

    private static async Task SeedGendersAsync(ApplicationDbContext context, string seedDataPath)
    {
        if (await context.Genders.AnyAsync()) return;

        var filePath = Path.Combine(seedDataPath, "Genders.json");
        if (!File.Exists(filePath)) return;

        var json = await File.ReadAllTextAsync(filePath);
        var genders = JsonSerializer.Deserialize<List<GenderSeedData>>(json, _jsonOptions);

        if (genders == null) return;

        var organization = await context.Organizations.FirstOrDefaultAsync();
        if (organization == null) return;

        var defaultBranchId = await GetDefaultBranchIdAsync(context);
        if (!defaultBranchId.HasValue) return;

        foreach (var genderData in genders)
        {
            var gender = new HrSystem.Domain.Entities.Employee.Gender
            {
                Id = genderData.Id,
                NameEn = genderData.NameEn,
                NameAr = genderData.NameAr,
                DisplayOrder = genderData.DisplayOrder,
                IsActive = genderData.IsActive,
                TenantId = organization.Id,
                BranchId = defaultBranchId.Value,
                CreatedDate = DateTimeOffset.UtcNow
            };

            await context.Genders.AddAsync(gender);
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"Seeded {genders.Count} genders");
    }

    private static async Task SeedMaritalStatusesAsync(ApplicationDbContext context, string seedDataPath)
    {
        if (await context.MaritalStatuses.AnyAsync()) return;

        var filePath = Path.Combine(seedDataPath, "MaritalStatuses.json");
        if (!File.Exists(filePath)) return;

        var json = await File.ReadAllTextAsync(filePath);
        var statuses = JsonSerializer.Deserialize<List<MaritalStatusSeedData>>(json, _jsonOptions);

        if (statuses == null) return;

        var organization = await context.Organizations.FirstOrDefaultAsync();
        if (organization == null) return;

        var defaultBranchId = await GetDefaultBranchIdAsync(context);
        if (!defaultBranchId.HasValue) return;

        foreach (var statusData in statuses)
        {
            var status = new HrSystem.Domain.Entities.Employee.MaritalStatus
            {
                Id = statusData.Id,
                NameEn = statusData.NameEn,
                NameAr = statusData.NameAr,
                DisplayOrder = statusData.DisplayOrder,
                IsActive = statusData.IsActive,
                TenantId = organization.Id,
                BranchId = defaultBranchId.Value,
                CreatedDate = DateTimeOffset.UtcNow
            };

            await context.MaritalStatuses.AddAsync(status);
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"Seeded {statuses.Count} marital statuses");
    }

    private static async Task SeedEmployeeStatusesAsync(ApplicationDbContext context, string seedDataPath)
    {
        if (await context.EmployeeStatuses.AnyAsync()) return;

        var filePath = Path.Combine(seedDataPath, "EmployeeStatuses.json");
        if (!File.Exists(filePath)) return;

        var json = await File.ReadAllTextAsync(filePath);
        var statuses = JsonSerializer.Deserialize<List<EmployeeStatusSeedData>>(json, _jsonOptions);

        if (statuses == null) return;

        var organization = await context.Organizations.FirstOrDefaultAsync();
        if (organization == null) return;

        var defaultBranchId = await GetDefaultBranchIdAsync(context);
        if (!defaultBranchId.HasValue) return;

        foreach (var statusData in statuses)
        {
            var status = new HrSystem.Domain.Entities.Employee.EmployeeStatus
            {
                Id = statusData.Id,
                NameEn = statusData.NameEn,
                NameAr = statusData.NameAr,
                Description = statusData.Description,
                ColorCode = statusData.ColorCode,
                DisplayOrder = statusData.DisplayOrder,
                IsActive = statusData.IsActive,
                TenantId = organization.Id,
                BranchId = defaultBranchId.Value,
                CreatedDate = DateTimeOffset.UtcNow
            };

            await context.EmployeeStatuses.AddAsync(status);
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"Seeded {statuses.Count} employee statuses");
    }

    private static async Task SeedPayrollStatusesAsync(ApplicationDbContext context, string seedDataPath)
    {
        if (await context.PayrollStatuses.AnyAsync()) return;

        var filePath = Path.Combine(seedDataPath, "PayrollStatuses.json");
        if (!File.Exists(filePath)) return;

        var json = await File.ReadAllTextAsync(filePath);
        var statuses = JsonSerializer.Deserialize<List<PayrollStatusSeedData>>(json, _jsonOptions);

        if (statuses == null) return;

        var organization = await context.Organizations.FirstOrDefaultAsync();
        if (organization == null) return;

        var defaultBranchId = await GetDefaultBranchIdAsync(context);
        if (!defaultBranchId.HasValue) return;

        foreach (var statusData in statuses)
        {
            var status = new HrSystem.Domain.Entities.Payroll.PayrollStatus
            {
                Id = statusData.Id,
                NameEn = statusData.NameEn,
                NameAr = statusData.NameAr,
                Description = statusData.Description,
                ColorCode = statusData.ColorCode,
                DisplayOrder = statusData.DisplayOrder,
                IsActive = statusData.IsActive,
                TenantId = organization.Id,
                BranchId = defaultBranchId.Value,
                CreatedDate = DateTimeOffset.UtcNow
            };

            await context.PayrollStatuses.AddAsync(status);
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"Seeded {statuses.Count} payroll statuses");
    }

    private static async Task SeedCountriesAsync(ApplicationDbContext context, string seedDataPath)
    {
        var filePath = Path.Combine(seedDataPath, "Countries.json");
        if (!File.Exists(filePath)) return;

        var json = await File.ReadAllTextAsync(filePath);
        var countries = JsonSerializer.Deserialize<List<CountrySeedData>>(json, _jsonOptions);

        if (countries == null || countries.Count == 0) return;

        var existingCountries = await context.Countries
            .IgnoreQueryFilters()
            .Select(c => new { c.Id, c.Code })
            .ToListAsync();

        var existingIds = new HashSet<Guid>(existingCountries.Select(c => c.Id));
        var existingCodes = new HashSet<string>(
            existingCountries
                .Where(c => !string.IsNullOrWhiteSpace(c.Code))
                .Select(c => c.Code!),
            StringComparer.OrdinalIgnoreCase);

        var addedCount = 0;

        foreach (var countryData in countries)
        {
            var countryId = countryData.Id == Guid.Empty ? Guid.NewGuid() : countryData.Id;
            var hasCode = !string.IsNullOrWhiteSpace(countryData.Code);

            if (existingIds.Contains(countryId) || (hasCode && existingCodes.Contains(countryData.Code!)))
            {
                continue;
            }

            var country = new HrSystem.Domain.Entities.Organization.Country
            {
                Id = countryId,
                NameEn = countryData.NameEn,
                NameAr = countryData.NameAr,
                Code = countryData.Code,
                Currency = countryData.Currency,
                TimeZone = countryData.TimeZone,
                PhoneCode = countryData.PhoneCode,
                DisplayOrder = countryData.DisplayOrder,
                IsActive = countryData.IsActive,
                CreatedDate = DateTimeOffset.UtcNow
            };

            await context.Countries.AddAsync(country);
            existingIds.Add(country.Id);
            if (hasCode)
            {
                existingCodes.Add(countryData.Code!);
            }
            addedCount++;
        }

        if (addedCount > 0)
        {
            await context.SaveChangesAsync();
            Console.WriteLine($"Seeded {addedCount} countries");
        }
    }
}
