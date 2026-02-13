using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Requests;

/// <summary>
/// Master data for Vacation types (Annual, Sick, Emergency, Unpaid, etc.)
/// Used for dropdown selection when submitting vacation requests.
/// </summary>
public class VacationType : BaseAuditableMasterEntity
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPaid { get; set; } = true;
    public int? MaxDaysPerYear { get; set; }
    public bool RequiresManagerApproval { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 1;

    public virtual ICollection<VacationRequestDetail> VacationRequests { get; set; } = new List<VacationRequestDetail>();
}
