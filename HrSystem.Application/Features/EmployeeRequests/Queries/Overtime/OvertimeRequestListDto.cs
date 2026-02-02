using HrSystem.Domain.Enums;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.Overtime;

public class OvertimeRequestListDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
    public string? EmployeeNameAr { get; set; }
    public string? DepartmentName { get; set; }
    public string? JobTitle { get; set; }
    public string? BranchName { get; set; }
    
    public Guid OvertimeTypeId { get; set; }
    public string? OvertimeTypeName { get; set; }
    public string? OvertimeTypeNameAr { get; set; }
    public DateTime OvertimeDate { get; set; }
    public TimeSpan PlannedHours { get; set; }
    public TimeSpan? ActualHours { get; set; }
    public decimal Multiplier { get; set; }
    public string? ProjectCode { get; set; }
    public string? TaskDescription { get; set; }
    
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public EmployeeRequestStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateTime RequestedDate { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? ApprovalNotes { get; set; }
    public string? RejectionReason { get; set; }
    
    public int CurrentApprovalLevel { get; set; }
}
