using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Performance;

public class GoalMilestone : BaseAuditableEntity
{
    public Guid GoalId { get; set; }
    public string TitleAr { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletionDate { get; set; }
    public string? Notes { get; set; }

    // Navigation Properties
    public virtual Goal Goal { get; set; } = null!;
}
