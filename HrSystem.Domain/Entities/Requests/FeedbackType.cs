using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Requests;

/// <summary>
/// Master data for Feedback types (Product Feedback, Process Improvement, Complaint, Suggestion, etc.)
/// Used for dropdown selection when submitting feedback requests.
/// </summary>
public class FeedbackType : BaseAuditableEntity
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsAnonymousAllowed { get; set; }
    public bool RequiresManagerApproval { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 1;

    public virtual ICollection<FeedbackRequestDetail> FeedbackRequests { get; set; } = new List<FeedbackRequestDetail>();
}
