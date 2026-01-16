using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Performance;

/// <summary>
/// Defines the possible statuses for employee goals.
/// This lookup table provides bilingual status options for goal tracking,
/// enables consistent status management across the system, and supports
/// reporting and filtering of goals by their current state.
/// </summary>
public class GoalStatus : BaseAuditableEntity
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public virtual ICollection<Goal> Goals { get; set; } = new List<Goal>();
}
