namespace HrSystem.Application.Features.Leave.Queries.GetLeaveRequests;

public class LeaveRequestDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeNameAr { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string? BranchName { get; set; }
    public Guid LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public string LeaveTypeNameAr { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalDays { get; set; }
    public string Reason { get; set; } = string.Empty;
    public Guid StatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string StatusNameAr { get; set; } = string.Empty;
    public DateTime? ManagerApprovalDate { get; set; }
    public string? ManagerComments { get; set; }
    public DateTime? HRApprovalDate { get; set; }
    public string? HRComments { get; set; }
    public string? DocumentUrl { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public bool RequiresHRApproval { get; set; }
    public string CurrentApprovalLevel { get; set; } = string.Empty;
}
