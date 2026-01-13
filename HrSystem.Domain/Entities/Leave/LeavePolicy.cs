using HrSystem.Domain.Common;
using HrSystem.Domain.Enums;

namespace HrSystem.Domain.Entities.Leave;

public class LeavePolicy : BaseAuditableEntity
{
    public LeaveType LeaveType { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public int DefaultDaysPerYear { get; set; }
    public int MaxCarryForward { get; set; }
    public bool RequiresApproval { get; set; } = true;
    public bool RequiresManagerApproval { get; set; } = true;
    public bool RequiresHRApproval { get; set; } = true;
    public bool IsPaid { get; set; } = true;
    public int MaxConsecutiveDays { get; set; }
    public int MinDaysNotice { get; set; }
    public bool RequiresDocument { get; set; }
    public string? Description { get; set; }

    // Navigation Properties
    public virtual ICollection<LeaveBalance> LeaveBalances { get; set; } = new List<LeaveBalance>();
}
