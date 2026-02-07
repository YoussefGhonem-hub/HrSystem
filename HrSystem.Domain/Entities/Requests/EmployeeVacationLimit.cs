using HrSystem.Domain.Common;
using HrSystem.Domain.Entities.Employee;
using EmployeeEntity = HrSystem.Domain.Entities.Employee.Employee;

namespace HrSystem.Domain.Entities.Requests;

/// <summary>
/// Overrides default vacation type limits (e.g., max annual leave days) for a specific employee.
/// </summary>
public class EmployeeVacationLimit : BaseAuditableEntity
{
    public Guid EmployeeId { get; set; }
    public Guid VacationTypeId { get; set; }
    public int? MaxDaysPerYear { get; set; }
    public string? Notes { get; set; }

    public virtual EmployeeEntity Employee { get; set; } = null!;
    public virtual VacationType VacationType { get; set; } = null!;
}
