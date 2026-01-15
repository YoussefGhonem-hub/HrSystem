using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Lifecycle;

/// <summary>
/// Manages structured onboarding process for new employees.
/// This entity ensures consistent new hire experiences, reduces time-to-productivity, prevents
/// missed critical setup tasks, and tracks onboarding progress. Essential for creating positive
/// first impressions, ensuring compliance with onboarding requirements, improving retention,
/// and helping new employees integrate smoothly into the organization.
/// </summary>
public class OnboardingTask : BaseAuditableEntity
{
    public Guid EmployeeId { get; set; }
    public string TaskNameAr { get; set; } = string.Empty;
    public string TaskNameEn { get; set; } = string.Empty;
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
    public int Sequence { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletionDate { get; set; }
    public Guid? AssignedTo { get; set; } // HR person responsible
    public string Category { get; set; } = string.Empty; // Documentation, SystemAccess, Training, etc.
    public string? Notes { get; set; }

    // Navigation Properties
    public virtual Employee.Employee Employee { get; set; } = null!;
}
