using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Organization;

public class OrganizationSettings : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public string SettingKey { get; set; } = string.Empty;
    public string SettingValue { get; set; } = string.Empty;
    public string? SettingType { get; set; } // String, Number, Boolean, JSON
    public string? Category { get; set; } // Payroll, Attendance, Leave, etc.
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }

    // Navigation Properties
    public virtual Organization Organization { get; set; } = null!;
}
