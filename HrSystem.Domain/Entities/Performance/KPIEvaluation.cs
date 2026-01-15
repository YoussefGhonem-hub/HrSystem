using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Performance;

/// <summary>
/// Records actual performance scores against defined KPIs during reviews.
/// This entity quantifies employee performance on specific metrics, enables weighted scoring
/// for overall performance calculations, provides evidence-based evaluation data, and supports
/// objective performance discussions. Essential for fair and transparent performance assessments
/// and defensible performance-based decisions.
/// </summary>
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
