using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Performance;

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
    
    public string Status { get; set; } = "NotStarted"; // NotStarted, InProgress, Completed, Cancelled
    public int Progress { get; set; } // 0-100 percentage
    public string Priority { get; set; } = "Medium"; // Low, Medium, High
    
    public Guid? AssignedBy { get; set; }
    public string? CompletionNotes { get; set; }

    // Navigation Properties
    public virtual Employee.Employee Employee { get; set; } = null!;
    public virtual ICollection<GoalMilestone> Milestones { get; set; } = new List<GoalMilestone>();
}
