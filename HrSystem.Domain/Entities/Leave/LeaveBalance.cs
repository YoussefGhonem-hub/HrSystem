using HrSystem.Domain.Common;
using HrSystem.Domain.Enums;

namespace HrSystem.Domain.Entities.Leave;

/// <summary>
/// Tracks available leave days for each employee by leave type and year.
/// This entity prevents unauthorized absences by enforcing leave entitlements, supports accurate
/// leave accrual calculations, and manages carry-forward provisions. Essential for maintaining
/// leave equity, ensuring compliance with employment contracts, and providing real-time visibility
/// into available leave balances for better workforce planning.
/// </summary>
public class LeaveBalance : BaseAuditableEntity
{
    public Guid EmployeeId { get; set; }
    public Guid LeavePolicyId { get; set; }
    public int Year { get; set; }
    public decimal TotalDays { get; set; }
    public decimal UsedDays { get; set; }
    public decimal RemainingDays { get; set; }
    public decimal CarriedForwardDays { get; set; }

    // Navigation Properties
    public virtual Employee.Employee Employee { get; set; } = null!;
    public virtual LeavePolicy LeavePolicy { get; set; } = null!;
}
