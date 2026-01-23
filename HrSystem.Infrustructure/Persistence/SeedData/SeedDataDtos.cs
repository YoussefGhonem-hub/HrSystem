using HrSystem.Domain.Entities.Attendance;
using HrSystem.Domain.Entities.Organization;
using HrSystem.Domain.Entities.Performance;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Infrustructure.Persistence.SeedData;

public class SeedDataDtos
{
    public class LeaveStatusSeedData
    {
        public Guid Id { get; set; }
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class RoleSeedData
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string NormalizedName { get; set; } = string.Empty;
    }

    public class LeaveTypeSeedData
    {
        public Guid Id { get; set; }
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public string? ColorCode { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }

    public class SubscriptionPlanSeedData
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

    public class OrganizationSeedData
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

    public class DepartmentSeedData
    {
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ParentDepartmentCode { get; set; }
        public string Code { get; set; } = string.Empty;
    }

    public class BranchSeedData
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

    public class EmployeeSeedData
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

    public class JobTitleSeedData
    {
        public string? Code { get; set; }
        public string TitleAr { get; set; } = string.Empty;
        public string TitleEn { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Level { get; set; }
        public decimal MinSalary { get; set; }
        public decimal MaxSalary { get; set; }
    }

    public class LeavePolicySeedData
    {
        public Guid LeaveTypeId { get; set; }
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

    public class AllowanceTypeSeedData
    {
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsTaxable { get; set; }
        public bool IsSubjectToInsurance { get; set; }
    }

    public class DeductionTypeSeedData
    {
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsRecurring { get; set; }
    }

    public class SocialInsuranceRateSeedData
    {
        public int Year { get; set; }
        public decimal EmployeeRate { get; set; }
        public decimal EmployerRate { get; set; }
        public decimal MinSalaryBase { get; set; }
        public decimal MaxSalaryBase { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
    }

    public class TaxBracketSeedData
    {
        public int Year { get; set; }
        public decimal MinIncome { get; set; }
        public decimal MaxIncome { get; set; }
        public decimal TaxRate { get; set; }
        public decimal FixedAmount { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
    }

    public class PublicHolidaySeedData
    {
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public int Year { get; set; }
        public bool IsRecurring { get; set; }
        public string? Description { get; set; }
    }

    public class WorkScheduleSeedData
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

    public class GoalStatusSeedData
    {
        public Guid Id { get; set; }
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string? DescriptionAr { get; set; }
        public string? DescriptionEn { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }

    public class GoalPrioritySeedData
    {
        public Guid Id { get; set; }
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string? DescriptionAr { get; set; }
        public string? DescriptionEn { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }

    public class StatusSeedData
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string? DescriptionAr { get; set; }
        public string? DescriptionEn { get; set; }
        public string? ColorCode { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }
}
