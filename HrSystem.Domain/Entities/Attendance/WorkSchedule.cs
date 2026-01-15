using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Attendance;

/// <summary>
/// Defines standard working hours and schedules for different employee groups.
/// This entity enables flexible work arrangement management, supports shift-based operations,
/// and establishes baseline expectations for attendance. Facilitates automated late/early leave
/// detection with grace periods, ensures fair attendance policies, and accommodates various
/// work patterns (e.g., 5-day, 6-day weeks, flexible hours).
/// </summary>
public class WorkSchedule : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public TimeSpan? BreakDuration { get; set; }
    public int WorkingHoursPerDay { get; set; } = 8;
    public int WorkingDaysPerWeek { get; set; } = 5;
    
    // Flexibility
    public TimeSpan? GracePeriodLate { get; set; } // e.g., 15 minutes
    public TimeSpan? GracePeriodEarlyLeave { get; set; }
    
    // Days
    public bool IsSaturday { get; set; } = true;
    public bool IsSunday { get; set; } = true;
    public bool IsMonday { get; set; } = true;
    public bool IsTuesday { get; set; } = true;
    public bool IsWednesday { get; set; } = true;
    public bool IsThursday { get; set; } = true;
    public bool IsFriday { get; set; } = false;
    
    public bool IsDefault { get; set; }

    // Navigation Properties
    public virtual ICollection<EmployeeWorkSchedule> EmployeeSchedules { get; set; } = new List<EmployeeWorkSchedule>();
}
