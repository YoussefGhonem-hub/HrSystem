using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Attendance;

public class OvertimeRequest : BaseAuditableEntity
{
    public Guid EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan Hours { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? ApprovalNotes { get; set; }

    // Navigation Properties
    public virtual Employee.Employee Employee { get; set; } = null!;
}
