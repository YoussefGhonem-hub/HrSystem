using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Requests;

/// <summary>
/// Master table for permission types (leave early, come late, short absence, etc.)
/// Used in dropdown selection when submitting permission requests.
/// </summary>
public class PermissionType : BaseAuditableEntity
{
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    /// <summary>
    /// Maximum allowed hours per single permission request
    /// </summary>
    public decimal? MaxHoursPerRequest { get; set; }
    
    /// <summary>
    /// Maximum allowed hours per month for this permission type
    /// </summary>
    public decimal? MaxHoursPerMonth { get; set; }
    
    /// <summary>
    /// Whether this permission deducts from leave balance
    /// </summary>
    public bool DeductsFromLeave { get; set; }
    
    /// <summary>
    /// If deducts from leave, how many hours equal one leave day
    /// </summary>
    public decimal? HoursPerLeaveDay { get; set; } = 8;
    
    public bool RequiresAttachment { get; set; }
    public bool RequiresManagerApproval { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    // Navigation
    public virtual ICollection<PermissionRequestDetail> PermissionRequests { get; set; } = new List<PermissionRequestDetail>();
}
