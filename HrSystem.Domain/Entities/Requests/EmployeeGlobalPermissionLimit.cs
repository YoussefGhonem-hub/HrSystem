using HrSystem.Domain.Common;
using EmployeeEntity = HrSystem.Domain.Entities.Employee.Employee;

namespace HrSystem.Domain.Entities.Requests;

/// <summary>
/// Defines a global monthly hours limit for ALL permission types combined for a specific employee.
/// This limit is checked before individual per-type limits.
/// </summary>
public class EmployeeGlobalPermissionLimit : BaseAuditableEntity
{
    /// <summary>
    /// The employee ID this limit applies to
    /// </summary>
    public Guid EmployeeId { get; set; }

    /// <summary>
    /// Total hours allowed per month across ALL permission types.
    /// If null, no global limit is enforced (only per-type limits apply).
    /// </summary>
    public decimal? TotalMonthlyHours { get; set; }

    /// <summary>
    /// Optional notes explaining this global limit
    /// </summary>
    public string? Notes { get; set; }

    public virtual EmployeeEntity Employee { get; set; } = null!;
}
