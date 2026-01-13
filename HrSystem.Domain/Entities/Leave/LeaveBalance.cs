using HrSystem.Domain.Common;
using HrSystem.Domain.Enums;

namespace HrSystem.Domain.Entities.Leave;

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
