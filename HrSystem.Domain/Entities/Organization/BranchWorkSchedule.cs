using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Organization;

/// <summary>
/// Links work schedules to specific branches with optional overrides.
/// Allows each branch to have its own default work schedule configuration,
/// enabling different working hours, days, and policies per location.
/// </summary>
public class BranchWorkSchedule : BaseAuditableEntity
{
    public new Guid BranchId { get; set; }
    
    // Schedule Name
    public string Name { get; set; } = "Default Schedule";
    
    // Working Hours
    public TimeSpan StartTime { get; set; } = new TimeSpan(9, 0, 0); // 09:00 AM
    public TimeSpan EndTime { get; set; } = new TimeSpan(17, 0, 0);  // 05:00 PM
    public TimeSpan? BreakDuration { get; set; } = new TimeSpan(1, 0, 0); // 1 hour
    public int WorkingHoursPerDay { get; set; } = 8;
    public int WorkingDaysPerWeek { get; set; } = 5;

    // Minimum Work Hours Policy
    public decimal ShiftTotalHours { get; set; } = 8.0m;
    public decimal MinimumFullDayHours { get; set; } = 6.0m;
    public decimal MinimumHalfDayHours { get; set; } = 3.0m;
    public decimal AbsentThresholdHours { get; set; } = 3.0m;
    public bool IsBreakTimeDeducted { get; set; } = false;
    public int? CheckInWindowMinutes { get; set; }
    public bool IsOvertimeEnabled { get; set; } = true;
    public decimal OvertimeStartsAfterHours { get; set; } = 8.0m;
    
    // Grace Periods
    public TimeSpan? GracePeriodLate { get; set; } = new TimeSpan(0, 15, 0); // 15 minutes
    public TimeSpan? GracePeriodEarlyLeave { get; set; } = new TimeSpan(0, 15, 0);
    
    // Working Days (true = working day)
    public bool IsSunday { get; set; } = true;
    public bool IsMonday { get; set; } = true;
    public bool IsTuesday { get; set; } = true;
    public bool IsWednesday { get; set; } = true;
    public bool IsThursday { get; set; } = true;
    public bool IsFriday { get; set; } = false;
    public bool IsSaturday { get; set; } = false;
    
    // Status
    public bool IsDefault { get; set; } = true;
    public bool IsActive { get; set; } = true;
    
    // Timezone for this schedule
    public string TimeZone { get; set; } = "Egypt Standard Time";
    
    // Navigation Properties
    public virtual Branch Branch { get; set; } = null!;
}
