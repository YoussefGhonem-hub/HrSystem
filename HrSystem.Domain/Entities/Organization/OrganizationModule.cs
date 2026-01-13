using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Organization;

public class OrganizationModule : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public string ModuleName { get; set; } = string.Empty; // Payroll, Attendance, Leave, Performance, etc.
    public bool IsEnabled { get; set; } = true;
    public DateTime EnabledDate { get; set; }
    public DateTime? DisabledDate { get; set; }
    public string? Configuration { get; set; } // JSON configuration for module

    // Navigation Properties
    public virtual Organization Organization { get; set; } = null!;
}
