using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Performance;

public class PerformanceReview : BaseAuditableEntity
{
    public Guid EmployeeId { get; set; }
    public Guid ReviewerId { get; set; }
    public DateTime ReviewPeriodStart { get; set; }
    public DateTime ReviewPeriodEnd { get; set; }
    public DateTime ReviewDate { get; set; }
    
    public string ReviewType { get; set; } = string.Empty; // Annual, Quarterly, Probation, etc.
    public decimal OverallRating { get; set; }
    public string Status { get; set; } = "Draft"; // Draft, Submitted, Approved, Completed
    
    public string? StrengthsAr { get; set; }
    public string? StrengthsEn { get; set; }
    public string? WeaknessesAr { get; set; }
    public string? WeaknessesEn { get; set; }
    public string? ImprovementAreasAr { get; set; }
    public string? ImprovementAreasEn { get; set; }
    public string? CommentsAr { get; set; }
    public string? CommentsEn { get; set; }
    
    public bool EmployeeAcknowledged { get; set; }
    public DateTime? EmployeeAcknowledgedDate { get; set; }
    public string? EmployeeComments { get; set; }

    // Navigation Properties
    public virtual Employee.Employee Employee { get; set; } = null!;
    public virtual Employee.Employee Reviewer { get; set; } = null!;
    public virtual ICollection<KPIEvaluation> KPIEvaluations { get; set; } = new List<KPIEvaluation>();
    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
}
