using HrSystem.Domain.Common;
using HrSystem.Domain.Entities.Account;
using HrSystem.Domain.Entities.Attendance;
using HrSystem.Domain.Entities.Employee;
using HrSystem.Domain.Entities.Leave;
using HrSystem.Domain.Entities.Organization;
using HrSystem.Domain.Entities.Payroll;
using HrSystem.Domain.Entities.Performance;
using HrSystem.Domain.Enums;
using HrSystem.Shared.Constants;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
            // Seed in order: Roles -> SubscriptionPlans -> Organization -> Branches -> Others
            await SeedRolesAsync(roleManager, seedDataPath);
            await SeedSubscriptionPlansAsync(context, seedDataPath);
            await SeedOrganizationAsync(context, seedDataPath);
            await SeedCountriesAsync(context, seedDataPath);
            await SeedBranchesAsync(context, seedDataPath);
            await SeedRoleUsersAsync(context, userManager, roleManager, seedDataPath);
            await SeedDepartmentsAsync(context, seedDataPath);
            await SeedJobTitlesAsync(context, seedDataPath);
            await SeedAttendanceStatusesAsync(context, seedDataPath);
            await SeedContractTypesAsync(context, seedDataPath);
            await SeedGendersAsync(context, seedDataPath);
            await SeedMaritalStatusesAsync(context, seedDataPath);
            await SeedEmployeeStatusesAsync(context, seedDataPath);
            await SeedEmployeesAsync(context, userManager, roleManager, seedDataPath);
            await SeedEmployeeDocumentTypesAsync(context, seedDataPath);
            await SeedLeaveStatusesAsync(context, seedDataPath);
            await SeedLeaveTypesAsync(context, seedDataPath);
            await SeedLeavePoliciesAsync(context, seedDataPath);
            await SeedAllowanceTypesAsync(context, seedDataPath);
            await SeedDeductionTypesAsync(context, seedDataPath);
            await SeedSocialInsuranceRatesAsync(context, seedDataPath);
            await SeedTaxBracketsAsync(context, seedDataPath);
            await SeedPublicHolidaysAsync(context, seedDataPath);
            await SeedWorkSchedulesAsync(context, seedDataPath);
            await SeedPayrollStatusesAsync(context, seedDataPath);
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

        var filePath = Path.Combine(seedDataPath, "Roles.json");
        if (!File.Exists(filePath)) return;

        var json = await File.ReadAllTextAsync(filePath);
        var roles = JsonSerializer.Deserialize<List<RoleSeedData>>(json, _jsonOptions);

        if (roles == null || roles.Count == 0) return;

        var branchRoleAdded = false;

        foreach (var roleData in roles)
        {
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

            var email = $"{roleData.Name.ToLowerInvariant()}@{organization.Code.ToLowerInvariant()}.local";
            var existingUser = await userManager.FindByEmailAsync(email);

            var user = existingUser ?? new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = roleData.DisplayName,
                IsActive = true,
                OrganizationId = organization.Id,
                CreatedDate = DateTimeOffset.UtcNow
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

            var inRole = await userManager.IsInRoleAsync(user, roleData.Name);
            if (!inRole)
            {
                await userManager.AddToRoleAsync(user, roleData.Name);
            }

            if (defaultBranch != null)
            {
                var assigned = await EnsureUserBranchRoleAsync(context, user.Id, defaultBranch.Id, roleData.Name);
                branchRoleAdded = branchRoleAdded || assigned;
            }

            Console.WriteLine($"Ensured role user for: {roleData.Name}");
        }

        if (branchRoleAdded)
        {
            await context.SaveChangesAsync();
        }
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

        // Dictionary to store employee codes and their IDs for manager assignment
        var employeeMap = new Dictionary<string, Guid>();

        // First pass: Create all employees without manager assignment
        foreach (var empData in employees)
        {
            var department = departments.FirstOrDefault(d => d.NameEn.StartsWith(empData.DepartmentCode));
            var jobTitle = jobTitles.FirstOrDefault(j => j.TitleEn.Contains(empData.JobTitleCode) || j.TitleEn.Replace(" ", "").ToUpper().Contains(empData.JobTitleCode.Replace("-", "")));
            var branch = branches.FirstOrDefault(b => b.Code == empData.BranchCode);

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

    private static async Task SeedEmployeeDocumentTypesAsync(ApplicationDbContext context, string seedDataPath)
    {
        if (await context.EmployeeDocumentTypes.AnyAsync()) return;

        var filePath = Path.Combine(seedDataPath, "EmployeeDocumentTypes.json");
        if (!File.Exists(filePath)) return;

        var json = await File.ReadAllTextAsync(filePath);
        var documentTypes = JsonSerializer.Deserialize<List<EmployeeDocumentTypeSeedData>>(json, _jsonOptions);

        if (documentTypes == null) return;

        var organization = await context.Organizations.FirstOrDefaultAsync();
        if (organization == null) return;

        var defaultBranchId = await GetDefaultBranchIdAsync(context);
        if (!defaultBranchId.HasValue) return;

        foreach (var docType in documentTypes)
        {
            var entity = new EmployeeDocumentType
            {
                Id = docType.Id == Guid.Empty ? Guid.NewGuid() : docType.Id,
                NameEn = docType.NameEn,
                NameAr = docType.NameAr,
                Description = docType.Description,
                CategoryKey = ParseDocumentCategory(docType.CategoryKey),
                DisplayOrder = docType.DisplayOrder,
                IsActive = docType.IsActive,
                TenantId = organization.Id,
                BranchId = defaultBranchId.Value,
                CreatedDate = DateTimeOffset.UtcNow
            };

            await context.EmployeeDocumentTypes.AddAsync(entity);
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"Seeded {documentTypes.Count} employee document types");
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

    private static DocumentCategory ParseDocumentCategory(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return DocumentCategory.Other;
        }

        return Enum.TryParse<DocumentCategory>(value, true, out var category)
            ? category
            : DocumentCategory.Other;
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

        var trimmed = roleName.Trim();
        if (RoleAliasMap.TryGetValue(trimmed, out var canonical))
        {
            return canonical;
        }

        var compressed = trimmed.Replace(" ", string.Empty);
        canonical = RoleNames.All.FirstOrDefault(r => r.Equals(trimmed, StringComparison.OrdinalIgnoreCase)
            || r.Equals(compressed, StringComparison.OrdinalIgnoreCase));
        return canonical;
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
