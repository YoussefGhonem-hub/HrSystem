using HrSystem.Domain.Common;
using HrSystem.Domain.Entities.Account;
using HrSystem.Domain.Enums;

namespace HrSystem.Domain.Entities.Requests;

/// <summary>
/// Unified self-service request entity for all employee requests.
/// Common fields live here; type-specific details are stored in child detail entities.
/// </summary>
public class EmployeeRequest : BaseAuditableEntity
{
    public EmployeeRequestType RequestType { get; set; }
    public EmployeeRequestStatus Status { get; set; } = EmployeeRequestStatus.Pending;

    public Guid EmployeeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime RequestedDate { get; set; } = DateTime.UtcNow;

    // Scheduling fields (shared across types)
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public string? AttachmentUrl { get; set; }
    public string? ManagerComments { get; set; }
    public string? RejectionReason { get; set; }

    // Approval workflow
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    
    // Processing workflow
    public Guid? ProcessedBy { get; set; }
    public DateTime? ProcessedDate { get; set; }

    // Navigation properties
    public virtual Employee.Employee Employee { get; set; } = null!;
    public virtual ApplicationUser? ApprovedByUser { get; set; }
    public virtual ApplicationUser? ProcessedByUser { get; set; }

    // Type-specific detail entities (one-to-one, based on RequestType)
    public virtual VacationRequestDetail? VacationDetail { get; set; }
    public virtual OvertimeRequestDetail? OvertimeDetail { get; set; }
    public virtual TrainingRequestDetail? TrainingDetail { get; set; }
    public virtual MiscellaneousRequestDetail? MiscellaneousDetail { get; set; }
    public virtual PersonalRequestDetail? PersonalDetail { get; set; }
    public virtual FeedbackRequestDetail? FeedbackDetail { get; set; }
    public virtual PermissionRequestDetail? PermissionDetail { get; set; }
}
