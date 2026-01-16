namespace HrSystem.Application.Features.Lifecycle.OffboardingTasks.Queries.GetOffboardingTasksList;

public record OffboardingTaskListDto
{
    public Guid Id { get; init; }
    public Guid EmployeeId { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public string TaskNameEn { get; init; } = string.Empty;
    public int Sequence { get; init; }
    public DateTime DueDate { get; init; }
    public bool IsCompleted { get; init; }
    public DateTime? CompletionDate { get; init; }
    public string Category { get; init; } = string.Empty;
}
