using HrSystem.Domain.Common;
using HrSystem.Domain.Enums;

namespace HrSystem.Domain.Entities.Attendance;

public class Attendance : BaseAuditableEntity
{
    public Guid EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan? CheckInTime { get; set; }
    public TimeSpan? CheckOutTime { get; set; }
    public AttendanceStatus Status { get; set; }
    
    // Biometric/Device Info
    public string? DeviceId { get; set; }
    public string? CheckInDeviceId { get; set; }
    public string? CheckOutDeviceId { get; set; }
    
    // Calculated Fields
    public TimeSpan? WorkedHours { get; set; }
    public TimeSpan? OvertimeHours { get; set; }
    public TimeSpan? LateMinutes { get; set; }
    public TimeSpan? EarlyLeaveMinutes { get; set; }
    
    public bool IsLate { get; set; }
    public bool IsEarlyLeave { get; set; }
    public bool IsOvertime { get; set; }
    
    public string? Notes { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }

    // Navigation Properties
    public virtual Employee.Employee Employee { get; set; } = null!;
}
