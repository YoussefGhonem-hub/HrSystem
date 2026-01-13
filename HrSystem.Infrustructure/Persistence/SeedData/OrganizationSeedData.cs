using HrSystem.Domain.Entities.Organization;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Infrustructure.Persistence.SeedData;

public static class OrganizationSeedData
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Seed Subscription Plans
        if (!await context.SubscriptionPlans.AnyAsync())
        {
            var plans = new List<SubscriptionPlan>
            {
                new SubscriptionPlan
                {
                    Id = Guid.NewGuid(),
                    Code = "BASIC",
                    NameAr = "الباقة الأساسية",
                    NameEn = "Basic Plan",
                    DescriptionAr = "مناسبة للشركات الصغيرة",
                    DescriptionEn = "Suitable for small companies",
                    MonthlyPrice = 500,
                    AnnualPrice = 5000,
                    MaxEmployees = 50,
                    MaxStorageGB = 10,
                    MaxDepartments = 10,
                    AllowBiometricIntegration = false,
                    AllowPayrollModule = true,
                    AllowPerformanceModule = false,
                    AllowRecruitmentModule = false,
                    AllowCustomReports = false,
                    AllowAPIAccess = false,
                    TrialDays = 30,
                    IsActive = true,
                    DisplayOrder = 1,
                    CreatedDate = DateTimeOffset.UtcNow
                },
                new SubscriptionPlan
                {
                    Id = Guid.NewGuid(),
                    Code = "PROFESSIONAL",
                    NameAr = "الباقة الاحترافية",
                    NameEn = "Professional Plan",
                    DescriptionAr = "مناسبة للشركات المتوسطة",
                    DescriptionEn = "Suitable for medium companies",
                    MonthlyPrice = 1500,
                    AnnualPrice = 15000,
                    MaxEmployees = 200,
                    MaxStorageGB = 50,
                    MaxDepartments = 50,
                    AllowBiometricIntegration = true,
                    AllowPayrollModule = true,
                    AllowPerformanceModule = true,
                    AllowRecruitmentModule = false,
                    AllowCustomReports = true,
                    AllowAPIAccess = false,
                    TrialDays = 30,
                    IsActive = true,
                    DisplayOrder = 2,
                    CreatedDate = DateTimeOffset.UtcNow
                },
                new SubscriptionPlan
                {
                    Id = Guid.NewGuid(),
                    Code = "ENTERPRISE",
                    NameAr = "باقة المؤسسات",
                    NameEn = "Enterprise Plan",
                    DescriptionAr = "مناسبة للشركات الكبيرة",
                    DescriptionEn = "Suitable for large enterprises",
                    MonthlyPrice = 5000,
                    AnnualPrice = 50000,
                    MaxEmployees = 1000,
                    MaxStorageGB = 500,
                    MaxDepartments = 500,
                    AllowBiometricIntegration = true,
                    AllowPayrollModule = true,
                    AllowPerformanceModule = true,
                    AllowRecruitmentModule = true,
                    AllowCustomReports = true,
                    AllowAPIAccess = true,
                    TrialDays = 30,
                    IsActive = true,
                    DisplayOrder = 3,
                    CreatedDate = DateTimeOffset.UtcNow
                }
            };

            await context.SubscriptionPlans.AddRangeAsync(plans);
            await context.SaveChangesAsync();
        }

        // Seed Demo Organization (Optional)
        if (!await context.Organizations.AnyAsync())
        {
            var basicPlan = await context.SubscriptionPlans.FirstAsync(p => p.Code == "BASIC");
            
            var demoOrganization = new Organization
            {
                Id = Guid.NewGuid(),
                Code = "DEMO001",
                NameAr = "شركة التجربة",
                NameEn = "Demo Company",
                Email = "info@democompany.com",
                PhoneNumber = "+201234567890",
                AddressAr = "القاهرة، مصر",
                AddressEn = "Cairo, Egypt",
                City = "Cairo",
                Country = "Egypt",
                SubscriptionPlanId = basicPlan.Id,
                SubscriptionStartDate = DateTime.UtcNow,
                SubscriptionEndDate = DateTime.UtcNow.AddDays(30),
                IsActive = true,
                IsTrialPeriod = true,
                TrialEndDate = DateTime.UtcNow.AddDays(30),
                MaxEmployees = 50,
                CurrentEmployeeCount = 0,
                MaxStorageGB = 10,
                CurrentStorageGB = 0,
                TimeZone = "Egypt Standard Time",
                DefaultLanguage = "ar",
                Currency = "EGP",
                DateFormat = "dd/MM/yyyy",
                CreatedDate = DateTimeOffset.UtcNow
            };

            await context.Organizations.AddAsync(demoOrganization);
            await context.SaveChangesAsync();

            // Seed default modules for demo organization
            var modules = new List<OrganizationModule>
            {
                new OrganizationModule
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = demoOrganization.Id,
                    ModuleName = "Employee Management",
                    IsEnabled = true,
                    EnabledDate = DateTime.UtcNow,
                    CreatedDate = DateTimeOffset.UtcNow
                },
                new OrganizationModule
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = demoOrganization.Id,
                    ModuleName = "Attendance",
                    IsEnabled = true,
                    EnabledDate = DateTime.UtcNow,
                    CreatedDate = DateTimeOffset.UtcNow
                },
                new OrganizationModule
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = demoOrganization.Id,
                    ModuleName = "Leave Management",
                    IsEnabled = true,
                    EnabledDate = DateTime.UtcNow,
                    CreatedDate = DateTimeOffset.UtcNow
                },
                new OrganizationModule
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = demoOrganization.Id,
                    ModuleName = "Payroll",
                    IsEnabled = true,
                    EnabledDate = DateTime.UtcNow,
                    CreatedDate = DateTimeOffset.UtcNow
                }
            };

            await context.OrganizationModules.AddRangeAsync(modules);
            await context.SaveChangesAsync();
        }
    }
}
