using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Lifecycle;

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
