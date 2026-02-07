using HrSystem.Domain.Common;
using HrSystem.Domain.Entities.Account;

namespace HrSystem.Domain.Entities.Requests;

/// <summary>
/// Type-specific detail for overtime employee requests.
/// Stores scheduling, multiplier, and approval tracking data for overtime submissions.
/// </summary>
public class OvertimeRequestDetail : BaseAuditableMasterEntity
{
    public Guid EmployeeRequestId { get; set; }
    public Guid OvertimeTypeId { get; set; }
    public DateTime OvertimeDate { get; set; }
    public TimeSpan PlannedHours { get; set; }
    public TimeSpan? ActualHours { get; set; }
    public decimal Multiplier { get; set; } = 1.5m;
    public string? ProjectCode { get; set; }
    public string? TaskDescription { get; set; }

    // Approval workflow
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? ApprovalNotes { get; set; }

    // Navigation
    public virtual EmployeeRequest EmployeeRequest { get; set; } = null!;
    public virtual OvertimeType OvertimeType { get; set; } = null!;
    public virtual ApplicationUser? ApprovedByUser { get; set; }
}
