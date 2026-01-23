namespace HrSystem.Application.Features.Performance.PerformanceReviews.Queries.GetPerformanceReviewById;

public record PerformanceReviewDto
{
    public Guid Id { get; init; }
    public Guid EmployeeId { get; init; }
    public string? EmployeeName { get; init; }
    public Guid ReviewerId { get; init; }
    public string? ReviewerName { get; init; }
    public DateTime ReviewPeriodStart { get; init; }
    public DateTime ReviewPeriodEnd { get; init; }
    public DateTime ReviewDate { get; init; }
    public Guid ReviewTypeId { get; init; }
    public string? ReviewType { get; init; }
    public decimal OverallRating { get; init; }
    public Guid StatusId { get; init; }
    public string? Status { get; init; }
    public string? StrengthsAr { get; init; }
    public string? StrengthsEn { get; init; }
    public string? WeaknessesAr { get; init; }
    public string? WeaknessesEn { get; init; }
    public string? ImprovementAreasAr { get; init; }
    public string? ImprovementAreasEn { get; init; }
    public string? CommentsAr { get; init; }
    public string? CommentsEn { get; init; }
    public bool EmployeeAcknowledged { get; init; }
    public DateTime? EmployeeAcknowledgedDate { get; init; }
    public string? EmployeeComments { get; init; }
}
