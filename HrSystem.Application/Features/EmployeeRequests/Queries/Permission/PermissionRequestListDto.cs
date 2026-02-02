using HrSystem.Domain.Enums;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.Permission;

public class PermissionRequestListDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeNameAr { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string? BranchName { get; set; }
    public Guid PermissionTypeId { get; set; }
    public string? PermissionTypeName { get; set; }
    public string? PermissionTypeNameAr { get; set; }
    public DateTime PermissionDate { get; set; }
    public TimeSpan? FromTime { get; set; }
    public TimeSpan? ToTime { get; set; }
    public decimal TotalHours { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public EmployeeRequestStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateTime? ManagerApprovalDate { get; set; }
    public string? ManagerComments { get; set; }
    public string? AttachmentUrl { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public string CurrentApprovalLevel { get; set; } = string.Empty;
}
