using HrSystem.Domain.Common;
using HrSystem.Domain.Enums;

namespace HrSystem.Domain.Entities.Leave;

/// <summary>
/// Defines organizational leave rules and entitlements for different leave types.
/// This entity standardizes leave management across the organization, ensures compliance with labor
/// laws and company policies, and provides clear guidelines for leave approval processes. Enables
/// consistent application of leave rules, supports audit requirements, and helps maintain fairness
/// in leave administration.
/// </summary>
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
