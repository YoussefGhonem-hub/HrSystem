using HrSystem.Domain.Enums;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.Feedback;

public class FeedbackRequestListDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
    public string? EmployeeNameAr { get; set; }
    public string? DepartmentName { get; set; }
    public string? JobTitle { get; set; }
    public string? BranchName { get; set; }
    
    public Guid FeedbackTypeId { get; set; }
    public string? FeedbackTypeName { get; set; }
    public string? FeedbackTypeNameAr { get; set; }
    public bool IsAnonymous { get; set; }
    public int? Rating { get; set; }
    public string? TargetDepartment { get; set; }
    public string? TargetPerson { get; set; }
    public bool ResponseRequired { get; set; }
    public string? ResponseContent { get; set; }
    public DateTime? ResponseDate { get; set; }
    
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public EmployeeRequestStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateTime RequestedDate { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? RejectionReason { get; set; }
    
    public int CurrentApprovalLevel { get; set; }
}
