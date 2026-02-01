using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Requests;

/// <summary>
/// Master data for Training types (Internal, External, Online, Certification, etc.)
/// Used for dropdown selection when submitting training requests.
/// </summary>
public class TrainingType : BaseAuditableEntity
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool RequiresBudgetApproval { get; set; }
    public bool RequiresManagerApproval { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 1;

    public virtual ICollection<TrainingRequestDetail> TrainingRequests { get; set; } = new List<TrainingRequestDetail>();
}
