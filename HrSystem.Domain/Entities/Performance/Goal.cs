using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Performance;

/// <summary>
/// Defines and tracks individual employee goals and objectives.
/// This entity aligns employee efforts with organizational objectives, enables progress monitoring,
/// supports performance-based evaluations, and drives accountability. Essential for maintaining
/// focus on priorities, facilitating meaningful performance discussions, and ensuring employees
/// contribute effectively to business success.
/// </summary>
public class Goal : BaseAuditableEntity
{
    public Guid EmployeeId { get; set; }
    public string TitleAr { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
    
    public DateTime StartDate { get; set; }
    public DateTime TargetDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    
    public Guid StatusId { get; set; }
    public int Progress { get; set; } // 0-100 percentage
    public Guid PriorityId { get; set; }
    
    public Guid? AssignedBy { get; set; }
    public string? CompletionNotes { get; set; }

    // Navigation Properties
    public virtual Employee.Employee Employee { get; set; } = null!;
    public virtual GoalStatus Status { get; set; } = null!;
    public virtual GoalPriority Priority { get; set; } = null!;
    public virtual ICollection<GoalMilestone> Milestones { get; set; } = new List<GoalMilestone>();
}
