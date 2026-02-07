using HrSystem.Domain.Common;
using HrSystem.Domain.Entities.Employee;
using EmployeeEntity = HrSystem.Domain.Entities.Employee.Employee;

namespace HrSystem.Domain.Entities.Requests;

/// <summary>
/// Overrides default permission type limits (e.g., monthly hours) for a specific employee.
/// </summary>
public class EmployeePermissionLimit : BaseAuditableEntity
{
    public Guid EmployeeId { get; set; }
    public Guid PermissionTypeId { get; set; }
    public decimal? MaxHoursPerMonth { get; set; }
    public string? Notes { get; set; }

    public virtual EmployeeEntity Employee { get; set; } = null!;
    public virtual PermissionType PermissionType { get; set; } = null!;
}
