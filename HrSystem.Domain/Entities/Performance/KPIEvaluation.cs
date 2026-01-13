using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Performance;

public class KPIEvaluation : BaseAuditableEntity
{
    public Guid PerformanceReviewId { get; set; }
    public Guid KPIId { get; set; }
    public decimal Rating { get; set; } // 1-5 scale
    public decimal WeightedScore { get; set; }
    public string? Comments { get; set; }
    public string? Evidence { get; set; }

    // Navigation Properties
    public virtual PerformanceReview PerformanceReview { get; set; } = null!;
    public virtual KPI KPI { get; set; } = null!;
}
