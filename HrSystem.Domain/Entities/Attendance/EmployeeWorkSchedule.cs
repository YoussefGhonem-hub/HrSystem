using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Attendance;

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
