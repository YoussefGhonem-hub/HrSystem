using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Requests;

/// <summary>
/// Master data for Miscellaneous request types (Government Paperwork, Equipment, IT Support, etc.)
/// Used for dropdown selection when submitting miscellaneous requests.
/// </summary>
public class MiscellaneousType : BaseAuditableMasterEntity
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool RequiresManagerApproval { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 1;

    public virtual ICollection<MiscellaneousRequestDetail> MiscellaneousRequests { get; set; } = new List<MiscellaneousRequestDetail>();
}
