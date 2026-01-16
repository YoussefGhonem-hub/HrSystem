namespace HrSystem.Application.Features.Performance.Feedbacks.Queries.GetFeedbacksList;

public class FeedbackListDto
{
    public Guid Id { get; set; }
    public Guid PerformanceReviewId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string FeedbackType { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public bool IsAnonymous { get; set; }
    public DateTime CreatedDate { get; set; }
}
