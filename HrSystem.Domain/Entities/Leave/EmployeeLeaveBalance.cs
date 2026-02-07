using HrSystem.Domain.Common;
using HrSystem.Domain.Entities.Requests;
using EmployeeEntity = HrSystem.Domain.Entities.Employee.Employee;

namespace HrSystem.Domain.Entities.Leave;

/// <summary>
/// Tracks yearly leave allocations per employee and vacation type (annual, sick, casual, etc.).
/// </summary>
public class EmployeeLeaveBalance : BaseAuditableEntity
{
    public Guid EmployeeId { get; set; }
    public Guid VacationTypeId { get; set; }
    public int Year { get; set; }

    public decimal AllocatedDays { get; set; }
    public decimal CarryOverDays { get; set; }
    public decimal ManualAdjustmentDays { get; set; }
    public decimal UsedDays { get; set; }
    public string? Notes { get; set; }

    public virtual EmployeeEntity Employee { get; set; } = null!;
    public virtual VacationType VacationType { get; set; } = null!;
    public virtual ICollection<EmployeeLeaveTransaction> Transactions { get; set; } = new List<EmployeeLeaveTransaction>();

    public decimal CalculateAvailableDays() => AllocatedDays + CarryOverDays + ManualAdjustmentDays - UsedDays;
}
