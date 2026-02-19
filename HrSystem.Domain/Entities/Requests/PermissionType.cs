using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Requests;

/// <summary>
/// Master table for permission types (leave early, come late, short absence, etc.)
/// Used in dropdown selection when submitting permission requests.
/// </summary>
public class PermissionType : BaseAuditableMasterEntity
{
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    public bool RequiresManagerApproval { get; set; } = true;
    public bool RequireAttachment { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    // Navigation
    public virtual ICollection<PermissionRequestDetail> PermissionRequests { get; set; } = new List<PermissionRequestDetail>();
}
