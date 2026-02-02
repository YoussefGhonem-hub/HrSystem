using HrSystem.Domain.Enums;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.Personal;

public class PersonalRequestListDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
    public string? EmployeeNameAr { get; set; }
    public string? DepartmentName { get; set; }
    public string? JobTitle { get; set; }
    public string? BranchName { get; set; }
    
    public Guid PersonalTypeId { get; set; }
    public string? PersonalTypeName { get; set; }
    public string? PersonalTypeNameAr { get; set; }
    public string? Reason { get; set; }
    public bool IsUrgent { get; set; }
    public bool RequiresConfidentiality { get; set; }
    
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
