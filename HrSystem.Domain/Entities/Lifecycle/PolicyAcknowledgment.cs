using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Lifecycle;

/// <summary>
/// Tracks employee acknowledgment of company policies and procedures.
/// This entity ensures legal compliance by documenting policy communication and acceptance,
/// reduces liability in disputes, maintains audit trails for compliance verification,
/// and ensures employees are aware of company rules. Essential for risk management,
/// regulatory compliance, and protecting the organization in legal matters.
/// </summary>
public class PolicyAcknowledgment : BaseAuditableEntity
{
    public Guid EmployeeId { get; set; }
    public string PolicyName { get; set; } = string.Empty;
    public string PolicyVersion { get; set; } = string.Empty;
    public DateTime AcknowledgedDate { get; set; }
    public string? DocumentPath { get; set; }
    public string? DocumentUrl { get; set; }
    public bool IsAcknowledged { get; set; }
    public string? EmployeeSignature { get; set; }
    public string? Notes { get; set; }

    // Navigation Properties
    public virtual Employee.Employee Employee { get; set; } = null!;
}
