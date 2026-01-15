using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Lifecycle;

/// <summary>
/// Manages structured offboarding process for departing employees.
/// This entity ensures proper exit procedures are followed, prevents security risks by revoking
/// access timely, facilitates asset recovery, and maintains compliance with termination protocols.
/// Critical for protecting company interests, maintaining security, ensuring knowledge transfer,
/// and creating professional departure experiences.
/// </summary>
public class OffboardingTask : BaseAuditableEntity
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
    public Guid? AssignedTo { get; set; }
    public string Category { get; set; } = string.Empty; // AssetReturn, AccessRevoke, Documentation, etc.
    public string? Notes { get; set; }

    // Navigation Properties
    public virtual Employee.Employee Employee { get; set; } = null!;
}
