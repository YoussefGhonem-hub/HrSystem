using HrSystem.Domain.Entities.Account;
using HrSystem.Domain.Entities.Attendance;
using HrSystem.Domain.Entities.Employee;
using HrSystem.Domain.Entities.Leave;
using HrSystem.Domain.Entities.Organization;
using HrSystem.Domain.Entities.Payroll;
using HrSystem.Domain.Entities.Performance;
using HrSystem.Domain.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace HrSystem.Infrustructure.Persistence;

public static class AppDbContextSeed
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
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
            await SeedBranchesAsync(context, seedDataPath);
            await SeedDepartmentsAsync(context, seedDataPath);
            await SeedJobTitlesAsync(context, seedDataPath);
            await SeedEmployeesAsync(context, userManager, seedDataPath);
            await SeedLeavePoliciesAsync(context, seedDataPath);
            await SeedAllowanceTypesAsync(context, seedDataPath);
            await SeedDeductionTypesAsync(context, seedDataPath);
            await SeedSocialInsuranceRatesAsync(context, seedDataPath);
            await SeedTaxBracketsAsync(context, seedDataPath);
            await SeedPublicHolidaysAsync(context, seedDataPath);
            await SeedWorkSchedulesAsync(context, seedDataPath);
            await SeedReviewTypesAsync(context, seedDataPath);
            await SeedReviewStatusesAsync(context, seedDataPath);
            await SeedGoalStatusesAsync(context, seedDataPath);
            await SeedGoalPrioritiesAsync(context, seedDataPath);
            await SeedOvertimeStatusesAsync(context, seedDataPath);
            await SeedInvoiceStatusesAsync(context, seedDataPath);

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
        if (await context.SubscriptionPlans.AnyAsync()) return;

        var filePath = Path.Combine(seedDataPath, "SubscriptionPlans.json");
        if (!File.Exists(filePath)) return;

        var json = await File.ReadAllTextAsync(filePath);
        var plans = JsonSerializer.Deserialize<List<SubscriptionPlanSeedData>>(json, _jsonOptions);

        if (plans == null) return;

        foreach (var planData in plans)
        {
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
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"Seeded {plans.Count} subscription plans");
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

        var organization = new Organization
        {
            Id = Guid.NewGuid(),
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
            CreatedDate = DateTimeOffset.UtcNow
        };

        await context.Organizations.AddAsync(organization);
        await context.SaveChangesAsync();
        Console.WriteLine($"Seeded demo organization: {orgData.NameEn}");
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
                Country = (Country)branchData.Country,
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
                CreatedDate = DateTimeOffset.UtcNow
            };

            await context.JobTitles.AddAsync(jobTitle);
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"Seeded {jobTitles.Count} job titles");
    }

    private static async Task SeedEmployeesAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, string seedDataPath)
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
                Gender = (Gender)empData.Gender,
                MaritalStatus = (MaritalStatus)empData.MaritalStatus,
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
                ContractType = (ContractType)empData.ContractType,
                Status = (EmployeeStatus)empData.Status,
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
            if (result.Succeeded && !string.IsNullOrEmpty(empData.Role))
            {
                await userManager.AddToRoleAsync(user, empData.Role);
                Console.WriteLine($"Created user: {empData.Email} with role: {empData.Role}");
            }

            employee.UserId = user.Id;
        }

        await context.SaveChangesAsync();

        // Second pass: Update manager assignments
        var allEmployees = await context.Employees.ToListAsync();
        foreach (var empData in employees.Where(e => !string.IsNullOrEmpty(e.DirectManagerCode)))
        {
            if (employeeMap.TryGetValue(empData.DirectManagerCode, out var managerId))
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

        foreach (var policyData in policies)
        {
            var policy = new LeavePolicy
            {
                Id = Guid.NewGuid(),
                LeaveType = (LeaveType)policyData.LeaveType,
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
                CreatedDate = DateTimeOffset.UtcNow
            };

            await context.DeductionTypes.AddAsync(deduction);
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"Seeded {deductions.Count} deduction types");
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
                CreatedDate = DateTimeOffset.UtcNow
            };

            await context.WorkSchedules.AddAsync(schedule);
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"Seeded {schedules.Count} work schedules");
    }

    // Seed Data DTOs
    private class RoleSeedData
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string NormalizedName { get; set; } = string.Empty;
    }

    private class SubscriptionPlanSeedData
    {
        public string Code { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string? DescriptionAr { get; set; }
        public string? DescriptionEn { get; set; }
        public decimal MonthlyPrice { get; set; }
        public decimal AnnualPrice { get; set; }
        public string Currency { get; set; } = "EGP";
        public int MaxEmployees { get; set; }
        public int MaxStorageGB { get; set; }
        public int MaxDepartments { get; set; }
        public bool AllowBiometricIntegration { get; set; }
        public bool AllowPayrollModule { get; set; }
        public bool AllowPerformanceModule { get; set; }
        public bool AllowRecruitmentModule { get; set; }
        public bool AllowCustomReports { get; set; }
        public bool AllowAPIAccess { get; set; }
        public int TrialDays { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
    }

    private class OrganizationSeedData
    {
        public string Code { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string? CommercialRegistrationNumber { get; set; }
        public string? TaxRegistrationNumber { get; set; }
        public string? LegalEntityType { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Website { get; set; }
        public string? AddressAr { get; set; }
        public string? AddressEn { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }
        public string SubscriptionPlanCode { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsTrialPeriod { get; set; }
        public int TrialDays { get; set; }
        public int MaxEmployees { get; set; }
        public int CurrentEmployeeCount { get; set; }
        public int MaxStorageGB { get; set; }
        public decimal CurrentStorageGB { get; set; }
        public string TimeZone { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public string? WeekStartDay { get; set; }
    }

    private class DepartmentSeedData
    {
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ParentDepartmentCode { get; set; }
        public string Code { get; set; } = string.Empty;
    }

    private class BranchSeedData
    {
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Country { get; set; }
        public string? City { get; set; }
        public string? AddressAr { get; set; }
        public string? AddressEn { get; set; }
        public string? PostalCode { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string TimeZone { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public string? Language { get; set; }
        public bool IsHeadquarter { get; set; }
        public bool IsActive { get; set; }
        public int MaxEmployeeCapacity { get; set; }
        public string WorkStartTime { get; set; } = string.Empty;
        public string WorkEndTime { get; set; } = string.Empty;
        public string? WorkingDays { get; set; }
    }

    private class EmployeeSeedData
    {
        public string EmployeeCode { get; set; } = string.Empty;
        public string FirstNameAr { get; set; } = string.Empty;
        public string LastNameAr { get; set; } = string.Empty;
        public string FirstNameEn { get; set; } = string.Empty;
        public string LastNameEn { get; set; } = string.Empty;
        public string NationalId { get; set; } = string.Empty;
        public string? PassportNumber { get; set; }
        public string DateOfBirth { get; set; } = string.Empty;
        public int Gender { get; set; }
        public int MaritalStatus { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? MobileNumber { get; set; }
        public string AddressAr { get; set; } = string.Empty;
        public string? AddressEn { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string DepartmentCode { get; set; } = string.Empty;
        public string JobTitleCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        public int ContractType { get; set; }
        public int Status { get; set; }
        public string HiringDate { get; set; } = string.Empty;
        public int ProbationPeriodMonths { get; set; }
        public string? DirectManagerCode { get; set; }
        public string? Role { get; set; }
    }

    private class JobTitleSeedData
    {
        public string? Code { get; set; }
        public string TitleAr { get; set; } = string.Empty;
        public string TitleEn { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Level { get; set; }
        public decimal MinSalary { get; set; }
        public decimal MaxSalary { get; set; }
    }

    private class LeavePolicySeedData
    {
        public int LeaveType { get; set; }
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public int DefaultDaysPerYear { get; set; }
        public int MaxCarryForward { get; set; }
        public bool RequiresApproval { get; set; }
        public bool RequiresManagerApproval { get; set; }
        public bool RequiresHRApproval { get; set; }
        public bool IsPaid { get; set; }
        public int MaxConsecutiveDays { get; set; }
        public int MinDaysNotice { get; set; }
        public bool RequiresDocument { get; set; }
        public string? Description { get; set; }
    }

    private class AllowanceTypeSeedData
    {
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsTaxable { get; set; }
        public bool IsSubjectToInsurance { get; set; }
    }

    private class DeductionTypeSeedData
    {
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsRecurring { get; set; }
    }

    private class SocialInsuranceRateSeedData
    {
        public int Year { get; set; }
        public decimal EmployeeRate { get; set; }
        public decimal EmployerRate { get; set; }
        public decimal MinSalaryBase { get; set; }
        public decimal MaxSalaryBase { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
    }

    private class TaxBracketSeedData
    {
        public int Year { get; set; }
        public decimal MinIncome { get; set; }
        public decimal MaxIncome { get; set; }
        public decimal TaxRate { get; set; }
        public decimal FixedAmount { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
    }

    private class PublicHolidaySeedData
    {
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public int Year { get; set; }
        public bool IsRecurring { get; set; }
        public string? Description { get; set; }
    }

    private class WorkScheduleSeedData
    {
        public string Name { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public string? BreakDuration { get; set; }
        public int WorkingHoursPerDay { get; set; }
        public int WorkingDaysPerWeek { get; set; }
        public string? GracePeriodLate { get; set; }
        public string? GracePeriodEarlyLeave { get; set; }
        public bool IsSaturday { get; set; }
        public bool IsSunday { get; set; }
        public bool IsMonday { get; set; }
        public bool IsTuesday { get; set; }
        public bool IsWednesday { get; set; }
        public bool IsThursday { get; set; }
        public bool IsFriday { get; set; }
        public bool IsDefault { get; set; }
    }

    private class GoalStatusSeedData
    {
        public Guid Id { get; set; }
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string? DescriptionAr { get; set; }
        public string? DescriptionEn { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }

    private class GoalPrioritySeedData
    {
        public Guid Id { get; set; }
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string? DescriptionAr { get; set; }
        public string? DescriptionEn { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }

    private class StatusSeedData
    {
        public string Code { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string? DescriptionAr { get; set; }
        public string? DescriptionEn { get; set; }
        public string? ColorCode { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }

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

        foreach (var typeData in reviewTypes)
        {
            var reviewType = new ReviewType
            {
                Id = Guid.NewGuid(),
                Code = typeData.Code,
                NameAr = typeData.NameAr,
                NameEn = typeData.NameEn,
                DescriptionAr = typeData.DescriptionAr,
                DescriptionEn = typeData.DescriptionEn,
                DisplayOrder = typeData.DisplayOrder,
                IsActive = typeData.IsActive,
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

        foreach (var statusData in reviewStatuses)
        {
            var reviewStatus = new ReviewStatus
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
                CreatedDate = DateTimeOffset.UtcNow
            };

            await context.InvoiceStatuses.AddAsync(invoiceStatus);
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"Seeded {invoiceStatuses.Count} invoice statuses");
    }
}