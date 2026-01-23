using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Performance;

/// <summary>
/// Defines the priority levels for employee goals.
/// This lookup table provides bilingual priority options for goal management,
/// enables consistent prioritization across the system, helps employees focus
/// on high-impact objectives, and supports resource allocation decisions.
/// </summary>
public class GoalPriority : BaseAuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
    public string? ColorCode { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public virtual ICollection<Goal> Goals { get; set; } = new List<Goal>();
}
