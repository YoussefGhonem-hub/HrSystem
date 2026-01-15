using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Performance;

/// <summary>
/// Breaks down goals into smaller, trackable milestones.
/// This entity facilitates progress tracking by creating checkpoints toward goal completion,
/// enables early identification of delays, supports agile goal management, and maintains motivation
/// through visible progress. Essential for effective goal execution and keeping employees
/// focused on incremental achievements.
/// </summary>
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
