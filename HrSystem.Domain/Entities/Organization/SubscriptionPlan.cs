using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Organization;

/// <summary>
/// Defines service tiers and pricing plans for the HR system.
/// This entity enables flexible monetization strategies by offering different feature sets and capacity
/// limits at various price points (Basic, Professional, Enterprise). Supports business growth through
/// tiered offerings, facilitates upselling opportunities, and allows organizations to choose plans
/// that match their size and requirements.
/// </summary>
public class SubscriptionPlan : BaseEntity
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty; // BASIC, PROFESSIONAL, ENTERPRISE
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
    
    // Pricing
    public decimal MonthlyPrice { get; set; }
    public decimal AnnualPrice { get; set; }
    public string Currency { get; set; } = "EGP";
    
    // Limits
    public int MaxEmployees { get; set; }
    public int MaxStorageGB { get; set; }
    public int MaxDepartments { get; set; }
    public bool AllowBiometricIntegration { get; set; }
    public bool AllowPayrollModule { get; set; }
    public bool AllowPerformanceModule { get; set; }
    public bool AllowRecruitmentModule { get; set; }
    public bool AllowCustomReports { get; set; }
    public bool AllowAPIAccess { get; set; }
    
    // Trial
    public int TrialDays { get; set; } = 30;
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }

    // Navigation Properties
    public virtual ICollection<Organization> Organizations { get; set; } = new List<Organization>();
}
