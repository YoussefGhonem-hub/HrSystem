using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Leave;

/// <summary>
/// Represents the status of a leave request in the approval workflow
/// </summary>
public class LeaveStatus : BaseEntity
{
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }    public bool IsActive { get; set; } = true;    
    // Navigation Properties
    public virtual ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
}
