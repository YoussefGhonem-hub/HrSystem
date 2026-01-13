using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Performance;

public class Feedback : BaseAuditableEntity
{
    public Guid PerformanceReviewId { get; set; }
    public Guid ProvidedBy { get; set; }
    public string FeedbackType { get; set; } = string.Empty; // Manager, Peer, Self, Subordinate
    public decimal Rating { get; set; }
    public string? Comments { get; set; }
    public bool IsAnonymous { get; set; }

    // Navigation Properties
    public virtual PerformanceReview PerformanceReview { get; set; } = null!;
    public virtual Employee.Employee Provider { get; set; } = null!;
}
