using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Attendance;

/// <summary>
/// Manages approval workflow for employee overtime work.
/// This entity ensures proper authorization and documentation of extra working hours, enabling accurate
/// overtime compensation calculation. Helps control labor costs, ensures compliance with overtime
/// regulations, maintains audit trails, and supports workforce planning by tracking overtime patterns.
/// </summary>
public class OvertimeRequest : BaseAuditableEntity
{
    public Guid EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan Hours { get; set; }
    public string Reason { get; set; } = string.Empty;
    public Guid StatusId { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? ApprovalNotes { get; set; }

    // Navigation Properties
    public virtual Employee.Employee Employee { get; set; } = null!;
    public virtual OvertimeStatus Status { get; set; } = null!;
}
