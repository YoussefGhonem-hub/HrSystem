using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Attendance;

/// <summary>
/// Links employees to their assigned work schedules with effective dates.
/// This entity enables schedule change tracking and maintains historical records of employee work
/// arrangements. Supports transition management when employees change shifts or departments,
/// and ensures accurate attendance evaluation based on the correct schedule for any given period.
/// </summary>
public class EmployeeWorkSchedule : BaseAuditableEntity
{
    public Guid EmployeeId { get; set; }
    public Guid WorkScheduleId { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsCurrent { get; set; } = true;

    // Navigation Properties
    public virtual Employee.Employee Employee { get; set; } = null!;
    public virtual WorkSchedule WorkSchedule { get; set; } = null!;
}
