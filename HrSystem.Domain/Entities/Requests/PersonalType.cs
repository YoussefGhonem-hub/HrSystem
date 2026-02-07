using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Requests;

/// <summary>
/// Master data for Personal request types (Family Emergency, Medical Appointment, Personal Matter, etc.)
/// Used for dropdown selection when submitting personal requests.
/// </summary>
public class PersonalType : BaseAuditableMasterEntity
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool RequiresManagerApproval { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 1;

    public virtual ICollection<PersonalRequestDetail> PersonalRequests { get; set; } = new List<PersonalRequestDetail>();
}
