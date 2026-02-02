using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Requests;

/// <summary>
/// Type-specific details for Training requests.
/// Links to TrainingType master and captures program info.
/// </summary>
public class TrainingRequestDetail : BaseAuditableEntity
{
    public Guid EmployeeRequestId { get; set; }
    
    // Training type from master
    public Guid TrainingTypeId { get; set; }
    public string TrainingName { get; set; } = string.Empty;
    public string? TrainingProvider { get; set; }
    public string? TrainingLocation { get; set; }
    
    public DateTime TrainingStartDate { get; set; }
    public DateTime TrainingEndDate { get; set; }
    public int DurationDays { get; set; }
    
    // Cost
    public decimal? EstimatedCost { get; set; }
    public decimal? ApprovedBudget { get; set; }
    public string? Currency { get; set; } = "EGP";
    
    // Justification
    public string? Objectives { get; set; }
    public string? ExpectedOutcome { get; set; }
    
    // Post-training
    public bool? CertificationObtained { get; set; }
    public string? CertificateUrl { get; set; }
    
    // Navigation
    public virtual EmployeeRequest EmployeeRequest { get; set; } = null!;
    public virtual TrainingType TrainingType { get; set; } = null!;
}
