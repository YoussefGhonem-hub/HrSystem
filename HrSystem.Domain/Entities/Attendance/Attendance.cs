using HrSystem.Domain.Common;
using HrSystem.Domain.Enums;

namespace HrSystem.Domain.Entities.Attendance;

/// <summary>
/// Records daily employee attendance with check-in and check-out times.
/// This entity enables accurate time tracking, monitors punctuality, and calculates worked hours for
/// payroll processing. Supports integration with biometric devices, identifies attendance patterns,
/// and helps reduce time theft. Critical for payroll accuracy, productivity analysis, and compliance
/// with labor regulations regarding working hours.
/// </summary>
public class Attendance : BaseAuditableEntity
{
    public Guid EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan? CheckInTime { get; set; }
    public TimeSpan? CheckOutTime { get; set; }
    public Guid StatusId { get; set; }
    public bool IsConfigurationRecord { get; set; }
    
    // Biometric/Device Info
    public string? DeviceId { get; set; }
    public string? CheckInDeviceId { get; set; }
    public string? CheckOutDeviceId { get; set; }
    
    // Location Info (GPS coordinates at time of check-in/out)
    public double? CheckInLatitude { get; set; }
    public double? CheckInLongitude { get; set; }
    public Guid? CheckInPointId { get; set; }
    public double? CheckOutLatitude { get; set; }
    public double? CheckOutLongitude { get; set; }
    public Guid? CheckOutPointId { get; set; }
    
    // Source tracking
    public AttendanceMethod? CheckInMethod { get; set; }
    public AttendanceMethod? CheckOutMethod { get; set; }
    
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

    // Attendance Configuration Snapshot
    public string? WorkShift { get; set; }
    public string? WorkDays { get; set; }
    public string? GracePeriod { get; set; }
    public string? MaxLatePerMonth { get; set; }
    public bool OvertimeEligible { get; set; }
    public string? OvertimeCalculation { get; set; }
    public string? AttendanceMethod { get; set; }
    public string? LateDeductionPolicy { get; set; }
    public string? AbsenceDeductionPolicy { get; set; }
    public string? HalfDayRule { get; set; }
    public string? MissingCheckoutHandling { get; set; }

    // Navigation Properties
    public virtual Employee.Employee Employee { get; set; } = null!;
    public virtual AttendanceStatus Status { get; set; } = null!;
    public virtual BranchCheckInPoint? CheckInPoint { get; set; }
    public virtual BranchCheckInPoint? CheckOutPoint { get; set; }
}
