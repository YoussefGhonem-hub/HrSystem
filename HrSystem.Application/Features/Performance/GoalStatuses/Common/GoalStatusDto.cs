namespace HrSystem.Application.Features.Performance.GoalStatuses.Common;

public record GoalStatusDto
{
    public Guid Id { get; init; }
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? DescriptionAr { get; init; }
    public string? DescriptionEn { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; }
    public int GoalsCount { get; init; }
    public DateTimeOffset CreatedDate { get; init; }
}
