using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Organization;

public class Organization : BaseEntity
{
    // Basic Information
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty; // Unique organization code
    public string? LogoUrl { get; set; }
    
    // Legal Information
    public string? CommercialRegistrationNumber { get; set; }
    public string? TaxRegistrationNumber { get; set; }
    public string? LegalEntityType { get; set; } // LLC, Corporation, etc.
    
    // Contact Information
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Website { get; set; }
    public string? AddressAr { get; set; }
    public string? AddressEn { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; } = "Egypt";
    public string? PostalCode { get; set; }
    
    // Subscription Information
    public Guid? SubscriptionPlanId { get; set; }
    public DateTime SubscriptionStartDate { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsTrialPeriod { get; set; }
    public DateTime? TrialEndDate { get; set; }
    
    // Limits
    public int MaxEmployees { get; set; } = 50;
    public int CurrentEmployeeCount { get; set; }
    public int MaxStorageGB { get; set; } = 10;
    public decimal CurrentStorageGB { get; set; }
    
    // Settings
    public string TimeZone { get; set; } = "Egypt Standard Time";
    public string DefaultLanguage { get; set; } = "ar";
    public string Currency { get; set; } = "EGP";
    public string DateFormat { get; set; } = "dd/MM/yyyy";
    
    // Database/Schema Information (for schema-per-tenant approach)
    public string? DatabaseName { get; set; }
    public string? ConnectionString { get; set; }
    
    // Audit
    public Guid? CreatedBy { get; set; }
    public Guid? ModifiedBy { get; set; }
    public DateTimeOffset? ModifiedDate { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedDate { get; set; }
    public Guid? DeletedBy { get; set; }

    // Navigation Properties
    public virtual SubscriptionPlan? SubscriptionPlan { get; set; }
    public virtual ICollection<OrganizationSettings> Settings { get; set; } = new List<OrganizationSettings>();
    public virtual ICollection<OrganizationModule> Modules { get; set; } = new List<OrganizationModule>();
    public virtual ICollection<Account.ApplicationUser> Users { get; set; } = new List<Account.ApplicationUser>();
}
