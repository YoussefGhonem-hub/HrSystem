namespace HrSystem.Application.Features.Performance.GoalMilestones.Queries.GetGoalMilestonesList;

public class GoalMilestoneListDto
{
    public Guid Id { get; set; }
    public Guid GoalId { get; set; }
    public string GoalTitleEn { get; set; } = string.Empty;
    public string GoalTitleAr { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletionDate { get; set; }
    public DateTime CreatedDate { get; set; }
}
