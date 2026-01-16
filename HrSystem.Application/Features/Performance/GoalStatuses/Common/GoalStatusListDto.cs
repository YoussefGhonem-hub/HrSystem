namespace HrSystem.Application.Features.Performance.GoalStatuses.Common;

public record GoalStatusListDto
{
    public Guid Id { get; init; }
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset CreatedDate { get; init; }
}
