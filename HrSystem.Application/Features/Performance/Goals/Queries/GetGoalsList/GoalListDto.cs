namespace HrSystem.Application.Features.Performance.Goals.Queries.GetGoalsList;

public record GoalListDto
{
    public Guid Id { get; init; }
    public Guid EmployeeId { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public string TitleAr { get; init; } = string.Empty;
    public string TitleEn { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime TargetDate { get; init; }
    public DateTime? CompletionDate { get; init; }
    public Guid StatusId { get; init; }
    public string StatusNameEn { get; init; } = string.Empty;
    public int Progress { get; init; }
    public Guid PriorityId { get; init; }
    public string PriorityNameEn { get; init; } = string.Empty;
}
