using HrSystem.Domain.Common;
using HrSystem.Domain.Enums;

namespace HrSystem.Domain.Entities.Leave;

/// <summary>
/// Audit trail for every change made to an employee's leave balance (allocation, deduction, adjustment).
/// </summary>
public class EmployeeLeaveTransaction : BaseAuditableEntity
{
    public Guid EmployeeLeaveBalanceId { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid VacationTypeId { get; set; }
    public int Year { get; set; }
    public LeaveTransactionType TransactionType { get; set; }
    public decimal DaysChanged { get; set; }
    public decimal BalanceAfter { get; set; }
    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? Notes { get; set; }

    public virtual EmployeeLeaveBalance LeaveBalance { get; set; } = null!;
}
