using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Performance;

/// <summary>
/// Collects multi-source feedback for comprehensive performance reviews.
/// This entity enables 360-degree feedback by gathering input from managers, peers, and subordinates,
/// provides holistic performance perspectives, supports development planning, and can maintain
/// anonymity when needed. Essential for well-rounded performance evaluations and identifying
/// blind spots in employee performance and behavior.
/// </summary>
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
