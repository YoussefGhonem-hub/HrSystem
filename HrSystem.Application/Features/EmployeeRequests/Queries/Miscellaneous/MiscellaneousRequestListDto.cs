using HrSystem.Domain.Enums;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.Miscellaneous;

public class MiscellaneousRequestListDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
    public string? EmployeeNameAr { get; set; }
    public string? DepartmentName { get; set; }
    public string? JobTitle { get; set; }
    public string? BranchName { get; set; }
    
    public Guid MiscellaneousTypeId { get; set; }
    public string? MiscellaneousTypeName { get; set; }
    public string? MiscellaneousTypeNameAr { get; set; }
    public string? AdditionalNotes { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Priority { get; set; }
    public DateTime? ExpectedCompletionDate { get; set; }
    
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public EmployeeRequestStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateTime RequestedDate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? RejectionReason { get; set; }
    
    public int CurrentApprovalLevel { get; set; }
}
