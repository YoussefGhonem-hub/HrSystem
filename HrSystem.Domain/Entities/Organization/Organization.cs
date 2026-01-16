using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Organization;

/// <summary>
/// Represents a company or organization using the HR system.
/// This entity enables multi-tenant SaaS architecture, allowing multiple companies to use the system
/// while maintaining complete data isolation. Manages subscription details, usage limits, and legal
/// information. Critical for business scalability, subscription management, and ensuring each
/// organization's data privacy and security.
/// </summary>
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
    public string Currency { get; set; } = "EGP";
    public string? WeekStartDay { get; set; }


    // Navigation Properties
    public virtual SubscriptionPlan? SubscriptionPlan { get; set; }
    public virtual ICollection<Branch> Branches { get; set; } = new List<Branch>();
    public virtual ICollection<Account.ApplicationUser> Users { get; set; } = new List<Account.ApplicationUser>();
}
