using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Requests;

/// <summary>
/// Type-specific details for Feedback requests.
/// Links to FeedbackType master.
/// </summary>
public class FeedbackRequestDetail : BaseAuditableMasterEntity
{
    public Guid EmployeeRequestId { get; set; }
    
    // Feedback type from master
    public Guid FeedbackTypeId { get; set; }
    public string FeedbackContent { get; set; } = string.Empty;
    public bool IsAnonymous { get; set; }
    public int? Rating { get; set; } // 1-5 scale if applicable
    public string? TargetDepartment { get; set; }
    public string? TargetPerson { get; set; }
    public string? SuggestedImprovement { get; set; }
    public bool ResponseRequired { get; set; }
    public string? ResponseContent { get; set; }
    public DateTime? ResponseDate { get; set; }
    
    // Navigation
    public virtual EmployeeRequest EmployeeRequest { get; set; } = null!;
    public virtual FeedbackType FeedbackType { get; set; } = null!;
}
