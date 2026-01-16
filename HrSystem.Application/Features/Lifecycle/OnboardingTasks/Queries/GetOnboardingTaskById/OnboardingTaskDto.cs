namespace HrSystem.Application.Features.Lifecycle.OnboardingTasks.Queries.GetOnboardingTaskById;

public record OnboardingTaskDto
{
    public Guid Id { get; init; }
    public Guid EmployeeId { get; init; }
    public string? EmployeeName { get; init; }
    public string TaskNameAr { get; init; } = string.Empty;
    public string TaskNameEn { get; init; } = string.Empty;
    public string? DescriptionAr { get; init; }
    public string? DescriptionEn { get; init; }
    public int Sequence { get; init; }
    public DateTime DueDate { get; init; }
    public bool IsCompleted { get; init; }
    public DateTime? CompletionDate { get; init; }
    public Guid? AssignedTo { get; init; }
    public string Category { get; init; } = string.Empty;
    public string? Notes { get; init; }
}
