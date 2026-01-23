namespace HrSystem.Application.Features.Performance.PerformanceReviews.Queries.GetPerformanceReviewsList;

public record PerformanceReviewListDto
{
    public Guid Id { get; init; }
    public Guid EmployeeId { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public Guid ReviewerId { get; init; }
    public string ReviewerName { get; init; } = string.Empty;
    public DateTime ReviewPeriodStart { get; init; }
    public DateTime ReviewPeriodEnd { get; init; }
    public DateTime ReviewDate { get; init; }
    public Guid ReviewTypeId { get; init; }
    public string? ReviewType { get; init; }
    public decimal OverallRating { get; init; }
    public Guid StatusId { get; init; }
    public string? Status { get; init; }
    public bool EmployeeAcknowledged { get; init; }
}
