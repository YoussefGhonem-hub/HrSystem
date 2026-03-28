using HrSystem.Domain.Common;
using HrSystem.Domain.Entities.Account;
using HrSystem.Domain.Entities.Attendance;
using HrSystem.Domain.Entities.Employee;
using HrSystem.Domain.Entities.Leave;
using HrSystem.Domain.Entities.Lifecycle;
using HrSystem.Domain.Entities.Organization;
using HrSystem.Domain.Entities.Payroll;
using HrSystem.Domain.Entities.Performance;
using HrSystem.Domain.Entities.Requests;
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
            await SeedSecondOrganizationAsync(context, seedDataPath);
            if (!await context.Organizations.AnyAsync())
            {
                Console.WriteLine("Organization seeding failed or organization already missing; aborting remaining seed steps.");
                return;
            }

            await SeedRolesAsync(roleManager, seedDataPath);
            await SeedCountriesAsync(context, seedDataPath);
            await SeedBranchesAsync(context, seedDataPath);
            await SeedSecondOrgBranchesAsync(context, seedDataPath);
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
            await SeedRequestTypeMastersAsync(context);
            await SeedBranchRequestSettingsAsync(context);
            await SeedBranchAttendanceSettingsAsync(context);

            if (await HasEmployeeSeedPrerequisitesAsync(context))
            {
                await SeedEmployeesAsync(context, userManager, roleManager, seedDataPath);
            }
            else
            {
                Console.WriteLine("Skipping employee seeding because prerequisite reference data is missing.");
            }

            await SeedRoleUsersAsync(context, userManager, roleManager, seedDataPath);
            await SeedSecondOrgUsersAsync(context, userManager, roleManager);
            await SeedEmployeeLeaveBalancesAsync(context);
            await SeedEmployeeDocumentsAsync(context);
            await SeedEmployeeRequestsAsync(context);
            await SeedSocialInsuranceRatesAsync(context, seedDataPath);
            await SeedTaxBracketsAsync(context, seedDataPath);
            await SeedPayrollStatusesAsync(context, seedDataPath);
            await SeedEmployeeSalaryAndPayrollHistoryAsync(context);
            await SeedAttendanceHistoryAsync(context);
            await SeedEmployeeAssetsAsync(context);
            await SeedReviewTypesAsync(context, seedDataPath);
            await SeedReviewStatusesAsync(context, seedDataPath);
            await SeedGoalStatusesAsync(context, seedDataPath);
            await SeedGoalPrioritiesAsync(context, seedDataPath);
            await SeedInvoiceStatusesAsync(context, seedDataPath);
            await EnsureDefaultScopeForSeedData(context);
            await EnsureDeptManagerDataAsync(context);

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

    private static async Task SeedSecondOrganizationAsync(ApplicationDbContext context, string seedDataPath)
    {
        var filePath = Path.Combine(seedDataPath, "DemoOrganization2.json");
        if (!File.Exists(filePath)) return;

        var json = await File.ReadAllTextAsync(filePath);
        var orgData = JsonSerializer.Deserialize<OrganizationSeedData>(json, _jsonOptions);

        if (orgData == null) return;

        // Skip if this organization already exists
        if (await context.Organizations.AnyAsync(o => o.Code == orgData.Code)) return;

        var plan = await context.SubscriptionPlans.FirstOrDefaultAsync(p => p.Code == orgData.SubscriptionPlanCode);
        if (plan == null)
        {
            Console.WriteLine($"Subscription plan '{orgData.SubscriptionPlanCode}' not found for second organization. Skipping.");
            return;
        }

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
        Console.WriteLine($"Seeded second organization: {orgData.NameEn}");
    }

    private static async Task SeedSecondOrgBranchesAsync(ApplicationDbContext context, string seedDataPath)
    {
        var filePath = Path.Combine(seedDataPath, "Branches2.json");
        if (!File.Exists(filePath)) return;

        var json = await File.ReadAllTextAsync(filePath);
        var branches = JsonSerializer.Deserialize<List<BranchSeedData>>(json, _jsonOptions);

        if (branches == null || branches.Count == 0) return;

        var organization = await context.Organizations.FirstOrDefaultAsync(o => o.Code == "ALPHA001");
        if (organization == null) return;

        // Skip if branches for this organization already exist
        if (await context.Branches.AnyAsync(b => b.OrganizationId == organization.Id)) return;

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
        Console.WriteLine($"Seeded {branches.Count} branches for {organization.NameEn}");
    }

    /// <summary>
    /// Seeds attendance settings for all organizations:
    /// - Demo Company (DEMO001): FaceId-based attendance with liveness detection.
    /// - Alpha Tech Solutions (ALPHA001): Location/GPS-based attendance with Cairo check-in points.
    /// </summary>
    private static async Task SeedBranchAttendanceSettingsAsync(ApplicationDbContext context)
    {
        var organizations = await context.Organizations.ToListAsync();
        var allBranches = await context.Branches.ToListAsync();
        var existingSettings = await context.BranchAttendanceSettings.ToListAsync();
        var existingCheckInPoints = await context.BranchCheckInPoints.ToListAsync();

        foreach (var org in organizations)
        {
            var orgBranches = allBranches.Where(b => b.OrganizationId == org.Id).ToList();
            if (orgBranches.Count == 0) continue;

            if (org.Code == "DEMO001")
            {
                await SeedOrUpdateFaceIdAttendanceAsync(context, org, orgBranches, existingSettings);
            }
            else if (org.Code == "ALPHA001")
            {
                await SeedOrUpdateLocationAttendanceAsync(context, org, orgBranches, existingSettings, existingCheckInPoints);
            }
        }

        Console.WriteLine("Seeded/updated branch attendance settings for all organizations.");
    }

    /// <summary>
    /// Seeds or updates FaceId attendance settings for Demo Company branches.
    /// </summary>
    private static async Task SeedOrUpdateFaceIdAttendanceAsync(
        ApplicationDbContext context,
        Organization org,
        List<Branch> branches,
        List<BranchAttendanceSetting> existingSettings)
    {
        var changed = false;

        foreach (var branch in branches)
        {
            var existing = existingSettings.FirstOrDefault(s => s.BranchId == branch.Id && !s.IsDeleted);

            if (existing != null)
            {
                // Update existing record to match FaceId-only config
                existing.PrimaryMethod = AttendanceMethod.FaceId;
                existing.AllowFaceId = true;
                existing.AllowLocation = false;
                existing.AllowExcelImport = false;
                existing.AllowFingerprint = false;
                existing.AllowManual = false;
                existing.RequireLocationValidation = false;
                existing.FaceIdConfidenceThreshold = 0.85;
                existing.FaceIdRequireLiveness = true;
                existing.AutoCheckoutEnabled = true;
                existing.AutoCheckoutTime = new TimeSpan(23, 59, 0);
                existing.Notes = "FaceId attendance with liveness detection enabled.";
                changed = true;
                Console.WriteLine($"  → Updated {branch.NameEn} to FaceId-only.");
            }
            else
            {
                var setting = new BranchAttendanceSetting
                {
                    Id = Guid.NewGuid(),
                    BranchId = branch.Id,
                    PrimaryMethod = AttendanceMethod.FaceId,
                    AllowFaceId = true,
                    AllowLocation = false,
                    AllowExcelImport = false,
                    AllowFingerprint = false,
                    AllowManual = false,
                    RequireLocationValidation = false,
                    DefaultGeofenceRadiusMeters = 200,
                    AutoCheckoutEnabled = true,
                    AutoCheckoutTime = new TimeSpan(23, 59, 0),
                    FaceIdConfidenceThreshold = 0.85,
                    FaceIdRequireLiveness = true,
                    ExcelImportSkipDuplicates = true,
                    AllowMultipleCheckInsPerDay = false,
                    MinCheckInDurationMinutes = 1,
                    Notes = "FaceId attendance with liveness detection enabled.",
                    TenantId = org.Id,
                    CreatedDate = DateTimeOffset.UtcNow
                };
                await context.BranchAttendanceSettings.AddAsync(setting);
                changed = true;
                Console.WriteLine($"  → Created FaceId setting for {branch.NameEn}.");
            }
        }

        if (changed)
        {
            await context.SaveChangesAsync();
            Console.WriteLine($"  → {org.NameEn}: FaceId attendance for {branches.Count} branch(es).");
        }
    }

    /// <summary>
    /// Seeds or updates Location/GPS attendance settings for Alpha Tech branches with Cairo check-in points.
    /// </summary>
    private static async Task SeedOrUpdateLocationAttendanceAsync(
        ApplicationDbContext context,
        Organization org,
        List<Branch> branches,
        List<BranchAttendanceSetting> existingSettings,
        List<BranchCheckInPoint> existingCheckInPoints)
    {
        // Cairo landmark coordinates for check-in points
        var cairoCheckInPoints = new Dictionary<string, (string NameAr, string NameEn, string Description, double Lat, double Lng, int Radius, string Address)[]>
        {
            ["ALPHA-HQ"] = new[]
            {
                ("البوابة الرئيسية - ميدان التحرير", "Main Gate - Tahrir Square",
                 "Primary entrance near Tahrir Square, Downtown Cairo",
                 30.0444, 31.2357, 150, "Tahrir Square, Downtown Cairo, Egypt"),

                ("المدخل الخلفي - كورنيش النيل", "Back Entrance - Nile Corniche",
                 "Secondary entrance on the Nile Corniche side",
                 30.0420, 31.2340, 100, "Nile Corniche, Downtown Cairo, Egypt")
            },
            ["ALPHA-NSR"] = new[]
            {
                ("المدخل الرئيسي - عباس العقاد", "Main Entrance - Abbas El-Akkad",
                 "Primary entrance on Abbas El-Akkad Street, Nasr City",
                 30.0511, 31.3462, 200, "Abbas El-Akkad Street, Nasr City, Cairo, Egypt"),

                ("بوابة الموظفين", "Staff Gate",
                 "Staff entrance from the side street",
                 30.0505, 31.3470, 100, "Side Street, Nasr City, Cairo, Egypt")
            }
        };

        foreach (var branch in branches)
        {
            var existing = existingSettings.FirstOrDefault(s => s.BranchId == branch.Id && !s.IsDeleted);
            Guid settingId;

            if (existing != null)
            {
                // Update existing record to match Location-only config
                existing.PrimaryMethod = AttendanceMethod.Location;
                existing.AllowFaceId = false;
                existing.AllowLocation = true;
                existing.AllowExcelImport = false;
                existing.AllowFingerprint = false;
                existing.AllowManual = false;
                existing.RequireLocationValidation = true;
                existing.DefaultGeofenceRadiusMeters = 200;
                existing.AutoCheckoutEnabled = true;
                existing.AutoCheckoutTime = new TimeSpan(23, 59, 0);
                existing.FaceIdRequireLiveness = false;
                existing.Notes = "Location/GPS-based attendance with geofenced Cairo check-in points.";
                settingId = existing.Id;
                Console.WriteLine($"  → Updated {branch.NameEn} to Location-only.");
            }
            else
            {
                settingId = Guid.NewGuid();
                var setting = new BranchAttendanceSetting
                {
                    Id = settingId,
                    BranchId = branch.Id,
                    PrimaryMethod = AttendanceMethod.Location,
                    AllowFaceId = false,
                    AllowLocation = true,
                    AllowExcelImport = false,
                    AllowFingerprint = false,
                    AllowManual = false,
                    RequireLocationValidation = true,
                    DefaultGeofenceRadiusMeters = 200,
                    AutoCheckoutEnabled = true,
                    AutoCheckoutTime = new TimeSpan(23, 59, 0),
                    FaceIdConfidenceThreshold = 0.85,
                    FaceIdRequireLiveness = false,
                    ExcelImportSkipDuplicates = true,
                    AllowMultipleCheckInsPerDay = false,
                    MinCheckInDurationMinutes = 1,
                    Notes = "Location/GPS-based attendance with geofenced Cairo check-in points.",
                    TenantId = org.Id,
                    CreatedDate = DateTimeOffset.UtcNow
                };
                await context.BranchAttendanceSettings.AddAsync(setting);
                Console.WriteLine($"  → Created Location setting for {branch.NameEn}.");
            }

            await context.SaveChangesAsync();

            // Seed check-in points if they don't exist for this branch
            var branchHasPoints = existingCheckInPoints.Any(p => p.BranchId == branch.Id && !p.IsDeleted);
            if (!branchHasPoints && cairoCheckInPoints.TryGetValue(branch.Code, out var points))
            {
                var displayOrder = 1;
                foreach (var (nameAr, nameEn, description, lat, lng, radius, address) in points)
                {
                    var checkInPoint = new BranchCheckInPoint
                    {
                        Id = Guid.NewGuid(),
                        BranchId = branch.Id,
                        BranchAttendanceSettingId = settingId,
                        NameAr = nameAr,
                        NameEn = nameEn,
                        Description = description,
                        Latitude = lat,
                        Longitude = lng,
                        RadiusMeters = radius,
                        IsCheckInPoint = true,
                        IsCheckOutPoint = true,
                        IsActive = true,
                        Address = address,
                        DisplayOrder = displayOrder++,
                        TenantId = org.Id,
                        CreatedDate = DateTimeOffset.UtcNow
                    };

                    await context.BranchCheckInPoints.AddAsync(checkInPoint);
                }

                await context.SaveChangesAsync();
                Console.WriteLine($"  → Added {points.Length} Cairo check-in points for {branch.NameEn}.");
            }
        }

        Console.WriteLine($"  → {org.NameEn}: Location attendance for {branches.Count} branch(es) with Cairo GPS check-in points.");
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

        // Determine starting counter based on existing employee codes to avoid duplicates
        var maxExistingCode = await context.Employees
            .Where(e => e.EmployeeCode != null && e.EmployeeCode.StartsWith("EMP-"))
            .Select(e => e.EmployeeCode)
            .ToListAsync();

        var employeeCodeCounter = 900;
        if (maxExistingCode.Count > 0)
        {
            var maxNum = maxExistingCode
                .Select(c => int.TryParse(c.Replace("EMP-", ""), out var n) ? n : 0)
                .DefaultIfEmpty(0)
                .Max();
            if (maxNum >= employeeCodeCounter)
                employeeCodeCounter = maxNum + 1;
        }

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

        // ── Assign DirectManagerId for employees in the DepartmentManager's branch ──
        // Find the DepartmentManager employee we just created
        var deptManagerRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == RoleNames.DepartmentManager);
        Employee? deptManagerEmployee = null;
        if (deptManagerRole != null)
        {
            var deptManagerUserId = await context.UserRoles
                .Where(ur => ur.RoleId == deptManagerRole.Id)
                .Join(context.Users.Where(u => u.EmployeeId != null),
                    ur => ur.UserId, u => u.Id, (ur, u) => u.EmployeeId!.Value)
                .FirstOrDefaultAsync();

            if (deptManagerUserId != Guid.Empty)
            {
                deptManagerEmployee = await context.Employees
                    .FirstOrDefaultAsync(e => e.Id == deptManagerUserId);
            }
        }

        if (deptManagerEmployee != null)
        {
            // Assign any employees in the same branch (and same org) that have no DirectManager
            var subordinates = await context.Employees
                .Where(e => e.BranchId == deptManagerEmployee.BranchId
                    && e.TenantId == deptManagerEmployee.TenantId
                    && e.Id != deptManagerEmployee.Id
                    && e.DirectManagerId == null)
                .ToListAsync();

            foreach (var sub in subordinates)
            {
                sub.DirectManagerId = deptManagerEmployee.Id;
            }

            if (subordinates.Any())
            {
                await context.SaveChangesAsync();
                Console.WriteLine($"Assigned {subordinates.Count} employees as subordinates of DepartmentManager ({deptManagerEmployee.EmployeeCode}).");
            }
        }

        Console.WriteLine("Finished creating default role-based users");
    }

    /// <summary>
    /// Seeds users for the second organization (Alpha Tech Solutions - ALPHA001).
    /// Creates an OrgAdmin + one HR Manager per branch, each with employee records.
    /// </summary>
    private static async Task SeedSecondOrgUsersAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        var organization = await context.Organizations.FirstOrDefaultAsync(o => o.Code == "ALPHA001");
        if (organization == null) return;

        var branches = await context.Branches
            .Where(b => b.OrganizationId == organization.Id)
            .OrderByDescending(b => b.IsHeadquarter)
            .ThenBy(b => b.CreatedDate)
            .ToListAsync();

        if (branches.Count == 0) return;

        var hqBranch = branches.FirstOrDefault(b => b.IsHeadquarter) ?? branches.First();

        var orgAdminEmail = $"admin@{organization.Code.ToLowerInvariant()}.local";

        var departments = await context.Departments.ToListAsync();
        var jobTitles = await context.JobTitles.ToListAsync();

        // Determine next available employee code
        var allCodes = await context.Employees
            .Where(e => e.EmployeeCode != null && e.EmployeeCode.StartsWith("EMP-"))
            .Select(e => e.EmployeeCode)
            .ToListAsync();

        var codeCounter = 1000;
        if (allCodes.Count > 0)
        {
            var maxNum = allCodes
                .Select(c => int.TryParse(c.Replace("EMP-", ""), out var n) ? n : 0)
                .DefaultIfEmpty(0)
                .Max();
            if (maxNum >= codeCounter)
                codeCounter = maxNum + 1;
        }

        const string defaultPassword = "Password@123";

        // ── 1) Organization Admin (org-wide, assigned to HQ branch) ──
        if (await userManager.FindByEmailAsync(orgAdminEmail) == null)
        {
            var orgAdminUser = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = orgAdminEmail,
                Email = orgAdminEmail,
                EmailConfirmed = true,
                FullName = "Khaled Mansour",
                IsActive = true,
                OrganizationId = organization.Id,
                BranchId = hqBranch.Id,
                CreatedDate = DateTimeOffset.UtcNow
            };

            var result = await userManager.CreateAsync(orgAdminUser, defaultPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(orgAdminUser, RoleNames.OrganizationAdmin);
                await EnsureUserBranchRoleAsync(context, orgAdminUser.Id, hqBranch.Id, RoleNames.OrganizationAdmin);
                Console.WriteLine($"  → Created OrgAdmin: {orgAdminEmail}");
            }
        }

        // ── 2) Per-branch users: HR Manager + Employee ──
        var branchUserDefinitions = new Dictionary<string, (string HrFullName, string HrFirstEn, string HrLastEn, string HrFirstAr, string HrLastAr,
                                                            string EmpFullName, string EmpFirstEn, string EmpLastEn, string EmpFirstAr, string EmpLastAr)>
        {
            ["ALPHA-HQ"] = ("Fatima Hassan", "Fatima", "Hassan", "فاطمة", "حسن",
                            "Youssef Ibrahim", "Youssef", "Ibrahim", "يوسف", "إبراهيم"),
            ["ALPHA-NSR"] = ("Nour El-Din", "Nour", "El-Din", "نور", "الدين",
                             "Mona Saeed", "Mona", "Saeed", "منى", "سعيد")
        };

        foreach (var branch in branches)
        {
            if (!branchUserDefinitions.TryGetValue(branch.Code, out var defs)) continue;

            Guid? hrEmployeeId = null;

            // ── HR Manager for this branch ──
            var hrEmail = $"hr.{branch.Code.ToLowerInvariant().Replace("-", "")}@{organization.Code.ToLowerInvariant()}.local";
            if (await userManager.FindByEmailAsync(hrEmail) == null)
            {
                var hrDepartment = departments.FirstOrDefault(d =>
                    d.NameEn.Contains("Human", StringComparison.OrdinalIgnoreCase))
                    ?? departments.FirstOrDefault();

                var hrJobTitle = jobTitles.FirstOrDefault(j =>
                    j.TitleEn.Contains("HR Manager", StringComparison.OrdinalIgnoreCase))
                    ?? jobTitles.FirstOrDefault();

                if (hrDepartment != null && hrJobTitle != null)
                {
                    var maleGenderId = Guid.Parse("00000000-0000-0000-0006-000000000001");
                    var femaleGenderId = Guid.Parse("00000000-0000-0000-0006-000000000002");
                    var singleMaritalStatusId = Guid.Parse("00000000-0000-0000-0007-000000000001");
                    var permanentContractTypeId = Guid.Parse("00000000-0000-0000-0005-000000000001");
                    var activeStatusId = Guid.Parse("00000000-0000-0000-0008-000000000001");

                    var hrEmployee = new Employee
                    {
                        Id = Guid.NewGuid(),
                        EmployeeCode = $"EMP-{codeCounter++:D4}",
                        FirstNameEn = defs.HrFirstEn,
                        LastNameEn = defs.HrLastEn,
                        FirstNameAr = defs.HrFirstAr,
                        LastNameAr = defs.HrLastAr,
                        NationalId = BuildAlphaNationalId(codeCounter),
                        DateOfBirth = DateTime.UtcNow.AddYears(-32),
                        GenderId = femaleGenderId,
                        MaritalStatusId = singleMaritalStatusId,
                        Email = hrEmail,
                        PhoneNumber = $"+2012000{codeCounter:D4}",
                        MobileNumber = $"+2010200{codeCounter:D5}",
                        AddressAr = "القاهرة، مصر",
                        AddressEn = "Cairo, Egypt",
                        DepartmentId = hrDepartment.Id,
                        JobTitleId = hrJobTitle.Id,
                        BranchId = branch.Id,
                        ContractTypeId = permanentContractTypeId,
                        StatusId = activeStatusId,
                        HiringDate = DateTime.UtcNow.AddYears(-2),
                        ProbationPeriodMonths = 0,
                        TenantId = organization.Id,
                        CreatedDate = DateTimeOffset.UtcNow
                    };

                    await context.Employees.AddAsync(hrEmployee);
                    await context.SaveChangesAsync();
                    hrEmployeeId = hrEmployee.Id;
                }

                var hrUser = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = hrEmail,
                    Email = hrEmail,
                    EmailConfirmed = true,
                    FullName = defs.HrFullName,
                    IsActive = true,
                    OrganizationId = organization.Id,
                    EmployeeId = hrEmployeeId,
                    BranchId = branch.Id,
                    CreatedDate = DateTimeOffset.UtcNow
                };

                var hrResult = await userManager.CreateAsync(hrUser, defaultPassword);
                if (hrResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(hrUser, RoleNames.HRManager);
                    await EnsureUserBranchRoleAsync(context, hrUser.Id, branch.Id, RoleNames.HRManager);

                    if (hrEmployeeId.HasValue)
                    {
                        var emp = await context.Employees.FindAsync(hrEmployeeId.Value);
                        if (emp != null) { emp.UserId = hrUser.Id; await context.SaveChangesAsync(); }
                    }

                    Console.WriteLine($"  → Created HR Manager for {branch.NameEn}: {hrEmail}");
                }
            }
            else if (!hrEmployeeId.HasValue)
            {
                // HR user already exists — resolve their employee ID for DirectManager linking
                var existingHrUser = await userManager.FindByEmailAsync(hrEmail);
                if (existingHrUser?.EmployeeId != null)
                    hrEmployeeId = existingHrUser.EmployeeId;
            }

            // ── Regular Employee for this branch ──
            var empEmail = $"emp.{branch.Code.ToLowerInvariant().Replace("-", "")}@{organization.Code.ToLowerInvariant()}.local";
            if (await userManager.FindByEmailAsync(empEmail) == null)
            {
                var empDepartment = departments.FirstOrDefault(d =>
                    d.NameEn.Contains("Operations", StringComparison.OrdinalIgnoreCase))
                    ?? departments.FirstOrDefault();

                var empJobTitle = jobTitles.FirstOrDefault(j =>
                    j.TitleEn.Contains("Employee", StringComparison.OrdinalIgnoreCase)
                    || j.TitleEn.Contains("Staff", StringComparison.OrdinalIgnoreCase))
                    ?? jobTitles.FirstOrDefault();

                Guid? regularEmployeeId = null;
                if (empDepartment != null && empJobTitle != null)
                {
                    var maleGenderId = Guid.Parse("00000000-0000-0000-0006-000000000001");
                    var marriedMaritalStatusId = Guid.Parse("00000000-0000-0000-0007-000000000002");
                    var permanentContractTypeId = Guid.Parse("00000000-0000-0000-0005-000000000001");
                    var activeStatusId = Guid.Parse("00000000-0000-0000-0008-000000000001");

                    var regularEmp = new Employee
                    {
                        Id = Guid.NewGuid(),
                        EmployeeCode = $"EMP-{codeCounter++:D4}",
                        FirstNameEn = defs.EmpFirstEn,
                        LastNameEn = defs.EmpLastEn,
                        FirstNameAr = defs.EmpFirstAr,
                        LastNameAr = defs.EmpLastAr,
                        NationalId = BuildAlphaNationalId(codeCounter),
                        DateOfBirth = DateTime.UtcNow.AddYears(-28),
                        GenderId = maleGenderId,
                        MaritalStatusId = marriedMaritalStatusId,
                        Email = empEmail,
                        PhoneNumber = $"+2012100{codeCounter:D4}",
                        MobileNumber = $"+2010210{codeCounter:D5}",
                        AddressAr = "القاهرة، مصر",
                        AddressEn = "Cairo, Egypt",
                        DepartmentId = empDepartment.Id,
                        JobTitleId = empJobTitle.Id,
                        BranchId = branch.Id,
                        DirectManagerId = hrEmployeeId,
                        ContractTypeId = permanentContractTypeId,
                        StatusId = activeStatusId,
                        HiringDate = DateTime.UtcNow.AddYears(-1),
                        ProbationPeriodMonths = 3,
                        TenantId = organization.Id,
                        CreatedDate = DateTimeOffset.UtcNow
                    };

                    await context.Employees.AddAsync(regularEmp);
                    await context.SaveChangesAsync();
                    regularEmployeeId = regularEmp.Id;
                }

                var empUser = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = empEmail,
                    Email = empEmail,
                    EmailConfirmed = true,
                    FullName = defs.EmpFullName,
                    IsActive = true,
                    OrganizationId = organization.Id,
                    EmployeeId = regularEmployeeId,
                    BranchId = branch.Id,
                    CreatedDate = DateTimeOffset.UtcNow
                };

                var empResult = await userManager.CreateAsync(empUser, defaultPassword);
                if (empResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(empUser, RoleNames.Employee);
                    await EnsureUserBranchRoleAsync(context, empUser.Id, branch.Id, RoleNames.Employee);

                    if (regularEmployeeId.HasValue)
                    {
                        var emp = await context.Employees.FindAsync(regularEmployeeId.Value);
                        if (emp != null) { emp.UserId = empUser.Id; await context.SaveChangesAsync(); }
                    }

                    Console.WriteLine($"  → Created Employee for {branch.NameEn}: {empEmail}");
                }
            }
        }

        await context.SaveChangesAsync();
        Console.WriteLine("Finished creating Alpha Tech users.");
    }

    private static async Task SeedEmployeeLeaveBalancesAsync(ApplicationDbContext context)
    {
        var hasEmployees = await context.Employees.AnyAsync();
        var hasVacationTypes = await context.VacationTypes.AnyAsync();

        if (!hasEmployees || !hasVacationTypes)
        {
            Console.WriteLine("Skipping leave balance seeding because employees or vacation types are missing.");
            return;
        }

        var currentYear = DateTime.UtcNow.Year;

        var employees = await context.Employees
            .AsNoTracking()
            .Select(e => new { e.Id, e.BranchId, e.TenantId })
            .ToListAsync();

        if (employees.Count == 0)
        {
            Console.WriteLine("No employees found for leave balance seeding.");
            return;
        }

        var vacationTypes = await context.VacationTypes
            .AsNoTracking()
            .Where(v => v.IsActive)
            .Select(v => new { v.Id, v.NameEn })
            .ToListAsync();

        if (vacationTypes.Count == 0)
        {
            Console.WriteLine("No active vacation types found for leave balance seeding.");
            return;
        }

        var existingPairs = await context.EmployeeLeaveBalances
            .AsNoTracking()
            .Where(b => b.Year == currentYear)
            .Select(b => new { b.EmployeeId, b.VacationTypeId })
            .ToListAsync();

        var existingSet = existingPairs
            .Select(p => (p.EmployeeId, p.VacationTypeId))
            .ToHashSet();

        var defaultAllocations = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            ["Annual Leave"] = 21m,
            ["Sick Leave"] = 14m,
            ["Emergency Leave"] = 7m,
            ["Unpaid Leave"] = 0m
        };

        var balancesToAdd = new List<EmployeeLeaveBalance>();

        foreach (var employee in employees)
        {
            foreach (var vacation in vacationTypes)
            {
                var key = (employee.Id, vacation.Id);
                if (existingSet.Contains(key))
                {
                    continue;
                }

                var allocatedDays = defaultAllocations.TryGetValue(vacation.NameEn, out var defaultDays) ? defaultDays : 0m;

                var balance = new EmployeeLeaveBalance
                {
                    EmployeeId = employee.Id,
                    VacationTypeId = vacation.Id,
                    Year = currentYear,
                    AllocatedDays = allocatedDays,
                    CarryOverDays = 0m,
                    ManualAdjustmentDays = 0m,
                    UsedDays = 0m,
                    TenantId = employee.TenantId,
                    BranchId = employee.BranchId,
                    Notes = "Initial allocation"
                };

                balancesToAdd.Add(balance);
            }
        }

        if (balancesToAdd.Count == 0)
        {
            Console.WriteLine($"Employee leave balances already exist for {currentYear}; skipping seeding.");
            return;
        }

        await context.EmployeeLeaveBalances.AddRangeAsync(balancesToAdd);
        await context.SaveChangesAsync();
        Console.WriteLine($"Seeded {balancesToAdd.Count} employee leave balances for {currentYear}.");
    }

    private static string BuildAlphaNationalId(int sequence)
    {
        var numericSegment = Math.Abs(sequence) % 1_000_000_000;
        return $"ALPHA{numericSegment:D9}";
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
        // Check if employee with this code already exists (idempotent re-run)
        var targetCode = $"EMP-{employeeCodeCounter:D4}";
        var existingEmployee = await context.Employees
            .FirstOrDefaultAsync(e => e.EmployeeCode == targetCode && !e.IsDeleted);
        if (existingEmployee != null)
        {
            Console.WriteLine($"Employee {targetCode} already exists for role {roleName}, reusing.");
            return existingEmployee.Id;
        }

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
                OrganizationId = organization.Id,
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
                OrganizationId = organization.Id,
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

    private static async Task SeedEmployeeRequestsAsync(ApplicationDbContext context)
    {
        if (await context.EmployeeRequests.IgnoreQueryFilters().AnyAsync())
        {
            return;
        }

        var employees = await context.Employees
            .AsNoTracking()
            .OrderBy(e => e.CreatedDate)
            .Select(e => new EmployeeRequestSeedScope(e.Id, e.TenantId, e.BranchId, e.UserId, e.DirectManagerId))
            .ToListAsync();

        if (employees.Count == 0)
        {
            Console.WriteLine("Skipping employee request seeding because no employees exist.");
            return;
        }

        var sampleEmployees = employees
            .Take(2)
            .ToList();

        var defaultBranchId = await GetDefaultBranchIdAsync(context);
        if (!defaultBranchId.HasValue)
        {
            Console.WriteLine("Skipping employee request seeding because no branch scope is available.");
            return;
        }

        var requestTypes = await context.RequestTypes
            .AsNoTracking()
            .ToDictionaryAsync(rt => rt.Code, rt => rt.Id, StringComparer.OrdinalIgnoreCase);

        var requiredCodes = new[] { "Vacation", "OverTime", "Training", "Miscellaneous", "Personal", "Feedback", "Permission" };
        if (requiredCodes.Any(code => !requestTypes.ContainsKey(code)))
        {
            Console.WriteLine("Skipping employee request seeding because one or more request types are missing.");
            return;
        }

        var vacationTypeId = await context.VacationTypes.AsNoTracking().Select(t => t.Id).FirstOrDefaultAsync();
        var overtimeTypeId = await context.OvertimeTypes.AsNoTracking().Select(t => t.Id).FirstOrDefaultAsync();
        var trainingTypeId = await context.TrainingTypes.AsNoTracking().Select(t => t.Id).FirstOrDefaultAsync();
        var miscellaneousTypeId = await context.MiscellaneousTypes.AsNoTracking().Select(t => t.Id).FirstOrDefaultAsync();
        var personalTypeId = await context.PersonalTypes.AsNoTracking().Select(t => t.Id).FirstOrDefaultAsync();
        var feedbackTypeId = await context.FeedbackTypes.AsNoTracking().Select(t => t.Id).FirstOrDefaultAsync();
        var permissionTypeId = await context.PermissionTypes.AsNoTracking().Select(t => t.Id).FirstOrDefaultAsync();

        if (new[] { vacationTypeId, overtimeTypeId, trainingTypeId, miscellaneousTypeId, personalTypeId, feedbackTypeId, permissionTypeId }.Any(id => id == Guid.Empty))
        {
            Console.WriteLine("Skipping employee request seeding because request detail master data is incomplete.");
            return;
        }

        var primaryEmployee = sampleEmployees[0];
        var secondaryEmployee = sampleEmployees.Count > 1 ? sampleEmployees[1] : sampleEmployees[0];
        var now = DateTimeOffset.UtcNow;

        Guid ResolveBranch(EmployeeRequestSeedScope scope) => scope.BranchId ?? defaultBranchId.Value;

        Guid ResolveManagerEmployeeId(EmployeeRequestSeedScope scope)
        {
            if (scope.DirectManagerId.HasValue)
            {
                return scope.DirectManagerId.Value;
            }

            var fallback = sampleEmployees.FirstOrDefault(e => e.Id != scope.Id) ?? scope;
            return fallback.Id;
        }

        Guid? ResolveManagerUserId(EmployeeRequestSeedScope scope)
        {
            var managerId = ResolveManagerEmployeeId(scope);
            var managerScope = employees.FirstOrDefault(e => e.Id == managerId);
            return managerScope?.UserId ?? scope.UserId;
        }

        DateTime ToDate(int daysOffset) => now.AddDays(daysOffset).UtcDateTime;
        DateTimeOffset ToOffset(int daysOffset) => now.AddDays(daysOffset);

        var employeeRequests = new List<EmployeeRequest>();
        var vacationDetails = new List<VacationRequestDetail>();
        var overtimeDetails = new List<OvertimeRequestDetail>();
        var trainingDetails = new List<TrainingRequestDetail>();
        var miscDetails = new List<MiscellaneousRequestDetail>();
        var personalDetails = new List<PersonalRequestDetail>();
        var feedbackDetails = new List<FeedbackRequestDetail>();
        var permissionDetails = new List<PermissionRequestDetail>();

        // Seed one request per status/type combination so UI filters return data immediately.
        var primaryBranchId = ResolveBranch(primaryEmployee);
        var primaryManagerUserId = ResolveManagerUserId(primaryEmployee);
        var primaryManagerEmployeeId = ResolveManagerEmployeeId(primaryEmployee);

        var vacationRequest = new EmployeeRequest
        {
            TenantId = primaryEmployee.TenantId,
            BranchId = primaryBranchId,
            EmployeeId = primaryEmployee.Id,
            RequestTypeId = requestTypes["Vacation"],
            Status = EmployeeRequestStatus.Approved,
            Title = "Annual leave for Eid",
            Description = "Five-day family trip scheduled around the Eid holiday.",
            RequestedDate = ToDate(-45),
            StartDate = ToDate(-30),
            EndDate = ToDate(-25),
            AttachmentUrl = "https://cdn.demo-hrsystem.local/seed/requests/vacation-eid.pdf",
            ManagerComments = "Enjoy your time off!",
            ApprovedBy = primaryManagerUserId,
            ApprovedDate = ToDate(-32),
            ProcessedBy = primaryManagerUserId,
            ProcessedDate = ToDate(-31),
            CreatedDate = ToOffset(-45)
        };
        employeeRequests.Add(vacationRequest);
        vacationDetails.Add(new VacationRequestDetail
        {
            EmployeeRequestId = vacationRequest.Id,
            VacationTypeId = vacationTypeId,
            TotalDays = 5,
            ManagerId = primaryManagerEmployeeId,
            ManagerApprovalDate = ToDate(-33),
            EmergencyContactName = "Layla Hassan",
            EmergencyContactPhone = "+20110001111",
            CreatedDate = ToOffset(-45)
        });

        var overtimeRequest = new EmployeeRequest
        {
            TenantId = primaryEmployee.TenantId,
            BranchId = primaryBranchId,
            EmployeeId = primaryEmployee.Id,
            RequestTypeId = requestTypes["OverTime"],
            Status = EmployeeRequestStatus.Pending,
            Title = "Quarter-end overtime support",
            Description = "Assist finance team with ERP closing adjustments.",
            RequestedDate = ToDate(-7),
            StartDate = ToDate(-6),
            EndDate = ToDate(-6),
            AttachmentUrl = "https://cdn.demo-hrsystem.local/seed/requests/overtime-q1.xlsx",
            CreatedDate = ToOffset(-7)
        };
        employeeRequests.Add(overtimeRequest);
        overtimeDetails.Add(new OvertimeRequestDetail
        {
            EmployeeRequestId = overtimeRequest.Id,
            OvertimeTypeId = overtimeTypeId,
            OvertimeDate = ToDate(-5),
            PlannedHours = TimeSpan.FromHours(4),
            ActualHours = null,
            Multiplier = 1.5m,
            ProjectCode = "FIN-Q1",
            TaskDescription = "Quarter-close reconciliations",
            CreatedDate = ToOffset(-7)
        });

        var trainingRequest = new EmployeeRequest
        {
            TenantId = primaryEmployee.TenantId,
            BranchId = primaryBranchId,
            EmployeeId = primaryEmployee.Id,
            RequestTypeId = requestTypes["Training"],
            Status = EmployeeRequestStatus.ManagerApproved,
            Title = "Advanced leadership workshop",
            Description = "External leadership lab to support succession planning.",
            RequestedDate = ToDate(-18),
            StartDate = ToDate(15),
            EndDate = ToDate(18),
            AttachmentUrl = "https://cdn.demo-hrsystem.local/seed/requests/training-outline.pdf",
            ManagerComments = "Aligned with development plan.",
            ApprovedBy = primaryManagerUserId,
            ApprovedDate = ToDate(-15),
            CreatedDate = ToOffset(-18)
        };
        employeeRequests.Add(trainingRequest);
        trainingDetails.Add(new TrainingRequestDetail
        {
            EmployeeRequestId = trainingRequest.Id,
            TrainingTypeId = trainingTypeId,
            TrainingName = "Advanced Leadership Lab",
            TrainingProvider = "AUC Executive Education",
            TrainingLocation = "New Cairo Campus",
            TrainingStartDate = ToDate(15),
            TrainingEndDate = ToDate(18),
            DurationDays = 4,
            EstimatedCost = 1800m,
            ApprovedBudget = 1800m,
            Currency = "USD",
            Objectives = "Strengthen coaching skills and executive presence.",
            ExpectedOutcome = "Employee to mentor upcoming team leads.",
            CertificationObtained = false,
            CreatedDate = ToOffset(-18)
        });

        var permissionRequest = new EmployeeRequest
        {
            TenantId = primaryEmployee.TenantId,
            BranchId = primaryBranchId,
            EmployeeId = primaryEmployee.Id,
            RequestTypeId = requestTypes["Permission"],
            Status = EmployeeRequestStatus.Completed,
            Title = "School orientation permission",
            Description = "Need to leave early for daughter's school orientation.",
            RequestedDate = ToDate(-4),
            StartDate = ToDate(-2),
            EndDate = ToDate(-2),
            ManagerComments = "Counts toward short leave quota.",
            ApprovedBy = primaryManagerUserId,
            ApprovedDate = ToDate(-3),
            ProcessedBy = primaryManagerUserId,
            ProcessedDate = ToDate(-1),
            CreatedDate = ToOffset(-4)
        };
        employeeRequests.Add(permissionRequest);
        permissionDetails.Add(new PermissionRequestDetail
        {
            EmployeeRequestId = permissionRequest.Id,
            PermissionTypeId = permissionTypeId,
            PermissionDate = ToDate(-2),
            FromTime = TimeSpan.FromHours(13),
            ToTime = TimeSpan.FromHours(16.5),
            TotalHours = 3.5m,
            Reason = "School orientation",
            ManagerId = primaryManagerEmployeeId,
            ManagerApprovalDate = ToDate(-3),
            ManagerComments = "Please log actual return time.",
            LeaveDeduction = 0.25m,
            CreatedDate = ToOffset(-4)
        });

        var secondaryBranchId = ResolveBranch(secondaryEmployee);
        var secondaryManagerUserId = ResolveManagerUserId(secondaryEmployee);

        var miscRequest = new EmployeeRequest
        {
            TenantId = secondaryEmployee.TenantId,
            BranchId = secondaryBranchId,
            EmployeeId = secondaryEmployee.Id,
            RequestTypeId = requestTypes["Miscellaneous"],
            Status = EmployeeRequestStatus.Draft,
            Title = "Travel visa paperwork",
            Description = "Need admin support to finalize client visit visa paperwork.",
            RequestedDate = ToDate(-1),
            CreatedDate = ToOffset(-1)
        };
        employeeRequests.Add(miscRequest);
        miscDetails.Add(new MiscellaneousRequestDetail
        {
            EmployeeRequestId = miscRequest.Id,
            MiscellaneousTypeId = miscellaneousTypeId,
            AdditionalNotes = "Embassy appointment booked for next Monday.",
            ReferenceNumber = $"TRV-{miscRequest.Id.ToString("N")[..6].ToUpperInvariant()}",
            Priority = "High",
            ExpectedCompletionDate = ToDate(12),
            CreatedDate = ToOffset(-1)
        });

        var personalRequest = new EmployeeRequest
        {
            TenantId = secondaryEmployee.TenantId,
            BranchId = secondaryBranchId,
            EmployeeId = secondaryEmployee.Id,
            RequestTypeId = requestTypes["Personal"],
            Status = EmployeeRequestStatus.Rejected,
            Title = "Family emergency travel",
            Description = "Requesting short unpaid leave to handle an urgent family surgery.",
            RequestedDate = ToDate(-20),
            StartDate = ToDate(-17),
            EndDate = ToDate(-14),
            AttachmentUrl = "https://cdn.demo-hrsystem.local/seed/requests/personal-emergency.pdf",
            ManagerComments = "Please attach hospital confirmation.",
            RejectionReason = "Missing supporting documentation.",
            ApprovedBy = secondaryManagerUserId,
            ApprovedDate = null,
            CreatedDate = ToOffset(-20)
        };
        employeeRequests.Add(personalRequest);
        personalDetails.Add(new PersonalRequestDetail
        {
            EmployeeRequestId = personalRequest.Id,
            PersonalTypeId = personalTypeId,
            Reason = "Urgent travel for family medical procedure",
            IsUrgent = true,
            RequiresConfidentiality = true,
            PreferredContactMethod = "Mobile",
            AdditionalContactInfo = "+97150000000",
            CreatedDate = ToOffset(-20)
        });

        var feedbackRequest = new EmployeeRequest
        {
            TenantId = secondaryEmployee.TenantId,
            BranchId = secondaryBranchId,
            EmployeeId = secondaryEmployee.Id,
            RequestTypeId = requestTypes["Feedback"],
            Status = EmployeeRequestStatus.Cancelled,
            Title = "Improve onboarding checklist",
            Description = "Suggestion to digitize onboarding tasks with automatic reminders.",
            RequestedDate = ToDate(-12),
            ManagerComments = "Duplicate of existing improvement initiative.",
            CreatedDate = ToOffset(-12)
        };
        employeeRequests.Add(feedbackRequest);
        feedbackDetails.Add(new FeedbackRequestDetail
        {
            EmployeeRequestId = feedbackRequest.Id,
            FeedbackTypeId = feedbackTypeId,
            FeedbackContent = "Add centralized onboarding checklist in Teams with auto-reminders.",
            IsAnonymous = false,
            Rating = 4,
            TargetDepartment = "Operations",
            TargetPerson = null,
            SuggestedImprovement = "Use Power Automate to assign tasks as soon as HR marks a new hire.",
            ResponseRequired = false,
            ResponseContent = null,
            CreatedDate = ToOffset(-12)
        });

        if (employeeRequests.Count == 0)
        {
            return;
        }

        await context.EmployeeRequests.AddRangeAsync(employeeRequests);

        if (vacationDetails.Count > 0) await context.VacationRequestDetails.AddRangeAsync(vacationDetails);
        if (overtimeDetails.Count > 0) await context.OvertimeRequestDetails.AddRangeAsync(overtimeDetails);
        if (trainingDetails.Count > 0) await context.TrainingRequestDetails.AddRangeAsync(trainingDetails);
        if (miscDetails.Count > 0) await context.MiscellaneousRequestDetails.AddRangeAsync(miscDetails);
        if (personalDetails.Count > 0) await context.PersonalRequestDetails.AddRangeAsync(personalDetails);
        if (feedbackDetails.Count > 0) await context.FeedbackRequestDetails.AddRangeAsync(feedbackDetails);
        if (permissionDetails.Count > 0) await context.PermissionRequestDetails.AddRangeAsync(permissionDetails);

        await context.SaveChangesAsync();
        Console.WriteLine($"Seeded {employeeRequests.Count} employee self-service requests across multiple types.");

        // ── Seed Pending requests from DeptManager's subordinates ──────────────
        // This ensures the DeptManager sees items in the "Pending Approval" tab.
        var deptManagerRole = await context.Roles.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name == RoleNames.DepartmentManager);
        if (deptManagerRole != null)
        {
            var deptManagerUserIds = await context.UserRoles.AsNoTracking()
                .Where(ur => ur.RoleId == deptManagerRole.Id)
                .Select(ur => ur.UserId)
                .ToListAsync();

            var deptManagerEmployeeIds = await context.Users.AsNoTracking()
                .Where(u => deptManagerUserIds.Contains(u.Id) && u.EmployeeId != null)
                .Select(u => u.EmployeeId!.Value)
                .ToListAsync();

            if (deptManagerEmployeeIds.Count > 0)
            {
                // Find subordinates of any DeptManager who don't already have requests
                var existingRequestEmployeeIds = await context.EmployeeRequests
                    .Select(er => er.EmployeeId)
                    .Distinct()
                    .ToListAsync();

                var subordinates = await context.Employees
                    .AsNoTracking()
                    .Where(e => e.DirectManagerId != null
                        && deptManagerEmployeeIds.Contains(e.DirectManagerId.Value)
                        && !deptManagerEmployeeIds.Contains(e.Id)
                        && !existingRequestEmployeeIds.Contains(e.Id))
                    .Select(e => new EmployeeRequestSeedScope(e.Id, e.TenantId, e.BranchId, e.UserId, e.DirectManagerId))
                    .Take(3)
                    .ToListAsync();

                var subRequests = new List<EmployeeRequest>();
                var subVacationDetails = new List<VacationRequestDetail>();

                foreach (var sub in subordinates)
                {
                    var subBranch = sub.BranchId ?? defaultBranchId.Value;

                    // Pending vacation request (awaiting manager first-level approval)
                    var pendingVacation = new EmployeeRequest
                    {
                        TenantId = sub.TenantId,
                        BranchId = subBranch,
                        EmployeeId = sub.Id,
                        RequestTypeId = requestTypes["Vacation"],
                        Status = EmployeeRequestStatus.Pending,
                        Title = "sick",
                        Description = "Feeling unwell, requesting sick leave.",
                        RequestedDate = ToDate(-2),
                        StartDate = ToDate(-1),
                        EndDate = ToDate(1),
                        CreatedDate = ToOffset(-2)
                    };
                    subRequests.Add(pendingVacation);
                    subVacationDetails.Add(new VacationRequestDetail
                    {
                        EmployeeRequestId = pendingVacation.Id,
                        VacationTypeId = vacationTypeId,
                        TotalDays = 3,
                        ManagerId = sub.DirectManagerId!.Value,
                        CreatedDate = ToOffset(-2)
                    });

                    // Another Pending request
                    var pendingOvertime = new EmployeeRequest
                    {
                        TenantId = sub.TenantId,
                        BranchId = subBranch,
                        EmployeeId = sub.Id,
                        RequestTypeId = requestTypes["OverTime"],
                        Status = EmployeeRequestStatus.Pending,
                        Title = "Weekend support shift",
                        Description = "Cover weekend production deployment.",
                        RequestedDate = ToDate(-1),
                        StartDate = ToDate(2),
                        EndDate = ToDate(2),
                        CreatedDate = ToOffset(-1)
                    };
                    subRequests.Add(pendingOvertime);
                    overtimeDetails.Add(new OvertimeRequestDetail
                    {
                        EmployeeRequestId = pendingOvertime.Id,
                        OvertimeTypeId = overtimeTypeId,
                        OvertimeDate = ToDate(2),
                        PlannedHours = TimeSpan.FromHours(6),
                        Multiplier = 1.5m,
                        ProjectCode = "OPS-WKD",
                        TaskDescription = "Weekend production deployment",
                        CreatedDate = ToOffset(-1)
                    });
                }

                if (subRequests.Count > 0)
                {
                    await context.EmployeeRequests.AddRangeAsync(subRequests);
                    if (subVacationDetails.Count > 0) await context.VacationRequestDetails.AddRangeAsync(subVacationDetails);
                    if (overtimeDetails.Count > 0) await context.OvertimeRequestDetails.AddRangeAsync(overtimeDetails);
                    await context.SaveChangesAsync();
                    Console.WriteLine($"Seeded {subRequests.Count} pending requests from DeptManager subordinates.");
                }
            }
        }
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

    private sealed record EmployeeDocumentSeedInfo(
        Guid Id,
        Guid TenantId,
        Guid? BranchId,
        string? EmployeeCode,
        string? FirstName,
        string? LastName,
        DateTime? HiringDate);

    private sealed record EmployeeRequestSeedScope(
        Guid Id,
        Guid TenantId,
        Guid? BranchId,
        Guid? UserId,
        Guid? DirectManagerId);

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

    /// <summary>
    /// Idempotent fixup that runs on every startup:
    /// 1. Ensures each DepartmentManager has subordinates (DirectManagerId) in their branch.
    /// 2. Ensures at least one Pending Vacation + Overtime request exists from those subordinates.
    /// </summary>
    private static async Task EnsureDeptManagerDataAsync(ApplicationDbContext context)
    {
        var deptManagerRole = await context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name == RoleNames.DepartmentManager);

        if (deptManagerRole == null)
        {
            Console.WriteLine("[EnsureDeptManagerData] DepartmentManager role not found – skipping.");
            return;
        }

        // Find ALL DepartmentManager employees across every tenant/org
        var deptManagerEmployeeIds = await context.UserRoles
            .Where(ur => ur.RoleId == deptManagerRole.Id)
            .Join(context.Users.Where(u => u.EmployeeId != null),
                ur => ur.UserId, u => u.Id, (ur, u) => u.EmployeeId!.Value)
            .ToListAsync();

        if (deptManagerEmployeeIds.Count == 0)
        {
            Console.WriteLine("[EnsureDeptManagerData] No DepartmentManager employees found – skipping.");
            return;
        }

        var deptManagers = await context.Employees
            .Where(e => deptManagerEmployeeIds.Contains(e.Id))
            .ToListAsync();

        // ── Step 1: Assign DirectManagerId where missing ──
        var assignedCount = 0;
        foreach (var mgr in deptManagers)
        {
            var subordinates = await context.Employees
                .Where(e => e.BranchId == mgr.BranchId
                    && e.TenantId == mgr.TenantId
                    && e.Id != mgr.Id
                    && e.DirectManagerId == null)
                .ToListAsync();

            foreach (var sub in subordinates)
            {
                sub.DirectManagerId = mgr.Id;
                assignedCount++;
            }
        }

        if (assignedCount > 0)
        {
            await context.SaveChangesAsync();
            Console.WriteLine($"[EnsureDeptManagerData] Assigned DirectManagerId on {assignedCount} employee(s).");
        }

        // ── Step 2: Ensure Pending requests exist for DeptManager subordinates ──
        var requestTypes = await context.RequestTypes
            .AsNoTracking()
            .ToDictionaryAsync(rt => rt.Code, rt => rt.Id, StringComparer.OrdinalIgnoreCase);

        if (!requestTypes.ContainsKey("Vacation") || !requestTypes.ContainsKey("OverTime"))
        {
            Console.WriteLine("[EnsureDeptManagerData] RequestTypes Vacation/OverTime not found – skipping request creation.");
            return;
        }

        var vacationTypeId = await context.VacationTypes.AsNoTracking().Select(t => t.Id).FirstOrDefaultAsync();
        var overtimeTypeId = await context.OvertimeTypes.AsNoTracking().Select(t => t.Id).FirstOrDefaultAsync();

        if (vacationTypeId == Guid.Empty || overtimeTypeId == Guid.Empty)
        {
            Console.WriteLine("[EnsureDeptManagerData] VacationType/OvertimeType not seeded – skipping request creation.");
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var newRequests = new List<EmployeeRequest>();
        var newVacationDetails = new List<VacationRequestDetail>();
        var newOvertimeDetails = new List<OvertimeRequestDetail>();

        foreach (var mgr in deptManagers)
        {
            // Get employees whose DirectManagerId == this manager
            var subs = await context.Employees
                .AsNoTracking()
                .Where(e => e.DirectManagerId == mgr.Id)
                .Select(e => new { e.Id, e.TenantId, e.BranchId })
                .ToListAsync();

            if (subs.Count == 0) continue;

            // Find which subordinates already have at least one Pending request
            var subsWithPending = await context.EmployeeRequests
                .AsNoTracking()
                .Where(r => r.Status == EmployeeRequestStatus.Pending
                    && subs.Select(s => s.Id).Contains(r.EmployeeId))
                .Select(r => r.EmployeeId)
                .Distinct()
                .ToListAsync();

            var subsNeedingRequests = subs.Where(s => !subsWithPending.Contains(s.Id)).ToList();
            if (subsNeedingRequests.Count == 0) continue;

            // Create Pending Vacation + Overtime for each subordinate that has none
            foreach (var sub in subsNeedingRequests)
            {
                var branchId = sub.BranchId ?? mgr.BranchId;

                // Pending Vacation Request
                var vacReq = new EmployeeRequest
                {
                    TenantId = sub.TenantId,
                    BranchId = branchId,
                    EmployeeId = sub.Id,
                    RequestTypeId = requestTypes["Vacation"],
                    Status = EmployeeRequestStatus.Pending,
                    Title = "Annual Leave Request",
                    Description = "Request for annual leave pending manager approval.",
                    RequestedDate = now.DateTime.AddDays(-2),
                    StartDate = now.DateTime.AddDays(5),
                    EndDate = now.DateTime.AddDays(10),
                    CreatedDate = now.AddDays(-2)
                };
                newRequests.Add(vacReq);

                newVacationDetails.Add(new VacationRequestDetail
                {
                    EmployeeRequestId = vacReq.Id,
                    VacationTypeId = vacationTypeId,
                    TotalDays = 5,
                    ManagerId = mgr.Id,
                    EmergencyContactName = "Emergency Contact",
                    EmergencyContactPhone = "+20100000000",
                    CreatedDate = now.AddDays(-2)
                });

                // Pending Overtime Request
                var otReq = new EmployeeRequest
                {
                    TenantId = sub.TenantId,
                    BranchId = branchId,
                    EmployeeId = sub.Id,
                    RequestTypeId = requestTypes["OverTime"],
                    Status = EmployeeRequestStatus.Pending,
                    Title = "Overtime Request",
                    Description = "Overtime work request pending manager approval.",
                    RequestedDate = now.DateTime.AddDays(-1),
                    StartDate = now.DateTime.AddDays(3),
                    EndDate = now.DateTime.AddDays(3),
                    CreatedDate = now.AddDays(-1)
                };
                newRequests.Add(otReq);

                newOvertimeDetails.Add(new OvertimeRequestDetail
                {
                    EmployeeRequestId = otReq.Id,
                    OvertimeTypeId = overtimeTypeId,
                    OvertimeDate = now.DateTime.AddDays(3),
                    PlannedHours = TimeSpan.FromHours(3),
                    Multiplier = 1.5m,
                    ProjectCode = "DEPT-OT",
                    TaskDescription = "Department overtime work",
                    CreatedDate = now.AddDays(-1)
                });
            }
        }

        if (newRequests.Count > 0)
        {
            await context.EmployeeRequests.AddRangeAsync(newRequests);
            await context.VacationRequestDetails.AddRangeAsync(newVacationDetails);
            await context.OvertimeRequestDetails.AddRangeAsync(newOvertimeDetails);
            await context.SaveChangesAsync();
            Console.WriteLine($"[EnsureDeptManagerData] Created {newRequests.Count} pending request(s) for DeptManager subordinates.");
        }
        else
        {
            Console.WriteLine("[EnsureDeptManagerData] All DeptManager subordinates already have pending requests – nothing to do.");
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

        // Build a set of already-seeded (Year, Month) pairs so we only add missing cycles
        var existingPeriods = new HashSet<(int Year, int Month)>(
            await context.PayrollCycles
                .AsNoTracking()
                .Select(c => new { c.Year, c.Month })
                .ToListAsync()
                .ContinueWith(t => t.Result.Select(x => (x.Year, x.Month))));

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

        for (var offset = 0; offset < 12; offset++)
        {
            var periodStart = new DateTime(today.Year, today.Month, 1).AddMonths(-offset);
            var periodEnd = periodStart.AddMonths(1).AddDays(-1);
            var isCurrent = offset == 0;

            // Skip if this cycle was already seeded
            if (existingPeriods.Contains((periodStart.Year, periodStart.Month)))
            {
                continue;
            }

            var cycle = new PayrollCycle
            {
                CycleName = periodStart.ToString("MMMM yyyy", CultureInfo.InvariantCulture),
                Month = periodStart.Month,
                Year = periodStart.Year,
                PeriodStartDate = periodStart,
                PeriodEndDate = periodEnd,
                PaymentDate = isCurrent ? null : periodEnd.AddDays(3),
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
                    IsPaid = !isCurrent,
                    PaidDate = isCurrent ? null : cycle.PaymentDate,
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
        var endDate = DateTime.UtcNow.Date;           // include today
        var startDate = endDate.AddDays(-60);          // ~2 months of history
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
                    checkIn = shiftStart;
                    checkOut = shiftStart;
                    worked = TimeSpan.Zero;
                }
                else
                {
                    var roll = random.NextDouble();
                    if (roll < 0.08)
                    {
                        statusId = absentStatusId;
                        checkIn = shiftStart;
                        checkOut = shiftStart;
                        worked = TimeSpan.Zero;
                    }
                    else if (roll < 0.12)
                    {
                        statusId = leaveStatusId;
                        checkIn = shiftStart;
                        checkOut = shiftEnd;
                        worked = TimeSpan.Zero;
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
                    CheckInDeviceId = "SEED-DEVICE",
                    CheckOutDeviceId = "SEED-DEVICE",
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

    private static async Task SeedRequestTypeMastersAsync(ApplicationDbContext context)
    {
        var now = DateTimeOffset.UtcNow;

        // Seed RequestTypes master (replaces enum values)
        if (!await context.RequestTypes.AnyAsync())
        {
            var requestTypes = new List<RequestType>
            {
                new() { Code = "Vacation", NameEn = "Vacation", NameAr = "إجازة", Description = "Days-based leave requests", IsActive = true, SortOrder = 1, CreatedDate = now, RequireAttachment = false },
                new() { Code = "OverTime", NameEn = "Overtime", NameAr = "وقت إضافي", Description = "Overtime work requests", IsActive = true, SortOrder = 2, CreatedDate = now, RequireAttachment = false },
                new() { Code = "Training", NameEn = "Training", NameAr = "تدريب", Description = "Training requests", IsActive = true, SortOrder = 3, CreatedDate = now, RequireAttachment = true },
                new() { Code = "Miscellaneous", NameEn = "Miscellaneous", NameAr = "متنوع", Description = "General purpose requests", IsActive = true, SortOrder = 4, CreatedDate = now, RequireAttachment = false },
                new() { Code = "Personal", NameEn = "Personal", NameAr = "شخصي", Description = "Personal requests", IsActive = true, SortOrder = 5, CreatedDate = now, RequireAttachment = false },
                new() { Code = "Feedback", NameEn = "Feedback", NameAr = "ملاحظات", Description = "Feedback submissions", IsActive = true, SortOrder = 6, CreatedDate = now, RequireAttachment = false },
                new() { Code = "Permission", NameEn = "Permission", NameAr = "إذن", Description = "Short absence / permission requests", IsActive = true, SortOrder = 7, CreatedDate = now, RequireAttachment = false }
            };

            await context.RequestTypes.AddRangeAsync(requestTypes);
            Console.WriteLine($"Seeded {requestTypes.Count} request types");
        }

        // Seed VacationTypes
        if (!await context.VacationTypes.AnyAsync())
        {
            var vacationTypes = new List<VacationType>
            {
                new()
                {
                    NameEn = "Annual Leave",
                    NameAr = "إجازة سنوية",
                    Description = "Paid annual leave covering standard vacation requests.",
                    IsPaid = true,
                    RequiresManagerApproval = true,
                    SortOrder = 1,
                    MaxDaysPerYear = 21,
                    CreatedDate = now
                },
                new()
                {
                    NameEn = "Sick Leave",
                    NameAr = "إجازة مرضية",
                    Description = "Medical leave that requires proof of illness.",
                    IsPaid = true,
                    RequiresManagerApproval = true,
                    RequireAttachment = true,
                    SortOrder = 2,
                    MaxDaysPerYear = 14,
                    CreatedDate = now
                },
                new()
                {
                    NameEn = "Unpaid Leave",
                    NameAr = "إجازة بدون راتب",
                    Description = "Leave without pay for exceptional cases.",
                    IsPaid = false,
                    RequiresManagerApproval = true,
                    SortOrder = 3,
                    MaxDaysPerYear = 0,
                    CreatedDate = now
                },
                new()
                {
                    NameEn = "Emergency Leave",
                    NameAr = "إجازة طارئة",
                    Description = "Short-term urgent leave for emergencies.",
                    IsPaid = true,
                    RequiresManagerApproval = true,
                    SortOrder = 4,
                    MaxDaysPerYear = 7,
                    CreatedDate = now
                }
            };
            await context.VacationTypes.AddRangeAsync(vacationTypes);
            Console.WriteLine($"Seeded {vacationTypes.Count} vacation types");
        }

        // Seed OvertimeTypes
        if (!await context.OvertimeTypes.AnyAsync())
        {
            var overtimeTypes = new List<OvertimeType>
            {
                new() { NameEn = "Regular Overtime", NameAr = "وقت إضافي عادي", Description = "Standard weekday overtime.", DefaultMultiplier = 1.5m, RequiresManagerApproval = true, SortOrder = 1, CreatedDate = now },
                new() { NameEn = "Weekend Overtime", NameAr = "وقت إضافي نهاية الأسبوع", Description = "Weekend overtime with higher multiplier.", DefaultMultiplier = 2.0m, RequiresManagerApproval = true, SortOrder = 2, CreatedDate = now },
                new() { NameEn = "Holiday Overtime", NameAr = "وقت إضافي إجازة رسمية", Description = "Public holiday overtime with premium rate.", DefaultMultiplier = 2.5m, RequiresManagerApproval = true, SortOrder = 3, CreatedDate = now }
            };
            await context.OvertimeTypes.AddRangeAsync(overtimeTypes);
            Console.WriteLine($"Seeded {overtimeTypes.Count} overtime types");
        }

        // Seed TrainingTypes
        if (!await context.TrainingTypes.AnyAsync())
        {
            var trainingTypes = new List<TrainingType>
            {
                new() { NameEn = "Internal Workshop", NameAr = "ورشة عمل داخلية", Description = "Training facilitated by in-house teams.", RequiresManagerApproval = true, SortOrder = 1, CreatedDate = now },
                new() { NameEn = "External Course", NameAr = "دورة خارجية", Description = "Paid training delivered by an external vendor.", RequiresManagerApproval = true, SortOrder = 2, CreatedDate = now },
                new() { NameEn = "Online Learning", NameAr = "تعلم إلكتروني", Description = "Self-paced online courses and certifications.", RequiresManagerApproval = true, SortOrder = 3, CreatedDate = now },
                new() { NameEn = "Conference/Seminar", NameAr = "مؤتمر/ندوة", Description = "Industry conferences and professional seminars.", RequiresManagerApproval = true, SortOrder = 4, CreatedDate = now }
            };
            await context.TrainingTypes.AddRangeAsync(trainingTypes);
            Console.WriteLine($"Seeded {trainingTypes.Count} training types");
        }

        // Seed MiscellaneousTypes
        if (!await context.MiscellaneousTypes.AnyAsync())
        {
            var miscTypes = new List<MiscellaneousType>
            {
                new() { NameEn = "Government Paperwork", NameAr = "معاملات حكومية", Description = "Official paperwork assistance requests.", RequiresManagerApproval = false, SortOrder = 1, CreatedDate = now },
                new() { NameEn = "Equipment Request", NameAr = "طلب معدات", Description = "Request for office equipment or supplies.", RequiresManagerApproval = true, SortOrder = 2, CreatedDate = now },
                new() { NameEn = "Travel Arrangement", NameAr = "ترتيبات السفر", Description = "Business travel arrangement requests.", RequiresManagerApproval = true, SortOrder = 3, CreatedDate = now },
                new() { NameEn = "Other", NameAr = "أخرى", Description = "General miscellaneous requests.", RequiresManagerApproval = false, SortOrder = 99, CreatedDate = now }
            };
            await context.MiscellaneousTypes.AddRangeAsync(miscTypes);
            Console.WriteLine($"Seeded {miscTypes.Count} miscellaneous types");
        }

        // Seed PersonalTypes
        if (!await context.PersonalTypes.AnyAsync())
        {
            var personalTypes = new List<PersonalType>
            {
                new() { NameEn = "Family Emergency", NameAr = "طارئ عائلي", Description = "Urgent personal/family situations.", RequiresManagerApproval = true, SortOrder = 1, CreatedDate = now },
                new() { NameEn = "Medical Appointment", NameAr = "موعد طبي", Description = "Personal medical appointments.", RequiresManagerApproval = true, SortOrder = 2, CreatedDate = now },
                new() { NameEn = "Personal Matter", NameAr = "أمور شخصية", Description = "General personal matters requiring time off.", RequiresManagerApproval = true, SortOrder = 3, CreatedDate = now }
            };
            await context.PersonalTypes.AddRangeAsync(personalTypes);
            Console.WriteLine($"Seeded {personalTypes.Count} personal types");
        }

        // Seed FeedbackTypes
        if (!await context.FeedbackTypes.AnyAsync())
        {
            var feedbackTypes = new List<FeedbackType>
            {
                new() { NameEn = "Product Feedback", NameAr = "ملاحظات المنتج", Description = "Ideas and improvements related to products.", IsAnonymousAllowed = true, RequiresManagerApproval = false, SortOrder = 1, CreatedDate = now },
                new() { NameEn = "Process Improvement", NameAr = "تحسين العمليات", Description = "Suggestions for process improvements.", IsAnonymousAllowed = true, RequiresManagerApproval = false, SortOrder = 2, CreatedDate = now },
                new() { NameEn = "Workplace Concern", NameAr = "شكوى بيئة العمل", Description = "Workplace environment and safety concerns.", IsAnonymousAllowed = true, RequiresManagerApproval = false, SortOrder = 3, CreatedDate = now },
                new() { NameEn = "Recognition", NameAr = "تقدير", Description = "Recognize colleagues for their contributions.", IsAnonymousAllowed = false, RequiresManagerApproval = false, SortOrder = 4, CreatedDate = now }
            };
            await context.FeedbackTypes.AddRangeAsync(feedbackTypes);
            Console.WriteLine($"Seeded {feedbackTypes.Count} feedback types");
        }

        // Seed PermissionTypes
        if (!await context.PermissionTypes.AnyAsync())
        {
            var permissionTypes = new List<PermissionType>
            {
                new() { NameEn = "Late Arrival", NameAr = "تأخر عن الدوام", Description = "Permission to arrive after scheduled start time.", RequiresManagerApproval = true, SortOrder = 1, CreatedDate = now },
                new() { NameEn = "Early Departure", NameAr = "الخروج مبكراً", Description = "Leave work before official end time for personal matters.", RequiresManagerApproval = true, SortOrder = 2, CreatedDate = now },
                new() { NameEn = "Short Leave", NameAr = "إجازة قصيرة", Description = "Short absence during working hours (e.g., paperwork).", RequiresManagerApproval = true, SortOrder = 3, CreatedDate = now },
                new() { NameEn = "Field Visit", NameAr = "زيارة ميدانية", Description = "Authorized out-of-office visit during working hours.", RequiresManagerApproval = true, SortOrder = 4, CreatedDate = now }
            };
            await context.PermissionTypes.AddRangeAsync(permissionTypes);
            Console.WriteLine($"Seeded {permissionTypes.Count} permission types");
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedBranchRequestSettingsAsync(ApplicationDbContext context)
    {
        var branches = await context.Branches.ToListAsync();
        if (branches.Count == 0) return;

        // Fetch master RequestTypes to get their IDs
        var requestTypes = await context.RequestTypes
            .IgnoreQueryFilters()
            .Where(r => !r.IsDeleted)
            .ToDictionaryAsync(r => r.Code, r => r.Id);

        if (requestTypes.Count == 0)
        {
            Console.WriteLine("No RequestTypes found in master table. Skipping BranchRequestSettings seed.");
            return;
        }

        var existingKeys = new HashSet<string>(
            await context.BranchRequestSettings
                .IgnoreQueryFilters()
                .Select(s => s.BranchId.HasValue ? $"{s.BranchId.Value}-{s.RequestTypeId}" : string.Empty)
                .ToListAsync());

        var settings = new List<BranchRequestSetting>();
        var now = DateTimeOffset.UtcNow;

        // Map enum names to master RequestType codes
        var enumCodeMapping = new Dictionary<EmployeeRequestType, string>
        {
            { EmployeeRequestType.Vacation, "Vacation" },
            { EmployeeRequestType.OverTime, "OverTime" },
            { EmployeeRequestType.Training, "Training" },
            { EmployeeRequestType.Miscellaneous, "Miscellaneous" },
            { EmployeeRequestType.Personal, "Personal" },
            { EmployeeRequestType.Feedback, "Feedback" },
            { EmployeeRequestType.Permission, "Permission" }
        };

        foreach (var branch in branches)
        {
            foreach (var mapping in enumCodeMapping)
            {
                var enumValue = mapping.Key;
                var code = mapping.Value;

                if (!requestTypes.TryGetValue(code, out var requestTypeId))
                    continue;

                var key = $"{branch.Id}-{requestTypeId}";
                if (existingKeys.Contains(key))
                    continue;

                var maxOpenRequests = enumValue switch
                {
                    EmployeeRequestType.Vacation => 2,
                    EmployeeRequestType.OverTime => 5,
                    EmployeeRequestType.Personal => 1,
                    _ => (int?)null
                };

                settings.Add(new BranchRequestSetting
                {
                    RequestTypeId = requestTypeId,
                    IsVisibleToEmployees = true,
                    AllowEmployeesToSubmit = true,
                    MaxOpenRequests = maxOpenRequests,
                    TenantId = branch.TenantId,
                    BranchId = branch.Id,
                    CreatedDate = now
                });
            }
        }

        if (settings.Count == 0) return;

        await context.BranchRequestSettings.AddRangeAsync(settings);
        await context.SaveChangesAsync();
        Console.WriteLine($"Seeded {settings.Count} branch request settings");
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
