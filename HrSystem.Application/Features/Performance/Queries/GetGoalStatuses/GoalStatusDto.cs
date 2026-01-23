namespace HrSystem.Application.Features.Performance.Queries.GetGoalStatuses;

public record GoalStatusDto
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = string.Empty;
    public string NameAr { get; init; } = string.Empty;
    public string? DescriptionEn { get; init; }
    public string? DescriptionAr { get; init; }
    public string? ColorCode { get; init; }
    public int DisplayOrder { get; init; }
}
