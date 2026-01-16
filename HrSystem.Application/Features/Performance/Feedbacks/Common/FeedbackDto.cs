namespace HrSystem.Application.Features.Performance.Feedbacks.Common;

public class FeedbackDto
{
    public Guid Id { get; set; }
    public Guid PerformanceReviewId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public Guid ProvidedBy { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string FeedbackType { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public string? Comments { get; set; }
    public bool IsAnonymous { get; set; }
    public DateTime CreatedDate { get; set; }
}
