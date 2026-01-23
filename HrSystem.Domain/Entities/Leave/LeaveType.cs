using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Leave;

/// <summary>
/// Represents types of leave available in the system
/// </summary>
public class LeaveType : BaseEntity
{
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? ColorCode { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation Properties
    public virtual ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
    public virtual ICollection<LeavePolicy> LeavePolicies { get; set; } = new List<LeavePolicy>();
}
