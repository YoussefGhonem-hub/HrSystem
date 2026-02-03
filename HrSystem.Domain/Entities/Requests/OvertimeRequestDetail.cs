using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Requests;

/// <summary>
/// Type-specific details for Overtime requests.
/// Links to OvertimeType master and tracks hours/approval.
/// </summary>
public class OvertimeRequestDetail : BaseAuditableEntity
{
    public Guid EmployeeRequestId { get; set; }
    
    // Overtime type from master
    public Guid OvertimeTypeId { get; set; }
    public DateTime OvertimeDate { get; set; }
    public TimeSpan PlannedHours { get; set; }
    public TimeSpan? ActualHours { get; set; }
    public decimal Multiplier { get; set; } = 1.5m;
    
    // Approval workflow
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? ApprovalNotes { get; set; }
    
    // Project reference (optional)
    public string? ProjectCode { get; set; }
    public string? TaskDescription { get; set; }
    
    // Navigation
    public virtual EmployeeRequest EmployeeRequest { get; set; } = null!;
    public virtual OvertimeType OvertimeType { get; set; } = null!;
}
