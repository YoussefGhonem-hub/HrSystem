using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Organization;

/// <summary>
/// Represents a country where the organization may have branches.
/// Supports multi-national HR operations across different regions with country-specific
/// settings like currency, timezone, and language preferences.
/// </summary>
public class Country : BaseEntity
{
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty; // ISO Country Code (e.g., EG, SA, AE)
    public string? Currency { get; set; }
    public string? TimeZone { get; set; }
    public string? PhoneCode { get; set; } // e.g., +20, +966
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public virtual ICollection<Branch> Branches { get; set; } = new List<Branch>();
}
