namespace HrSystem.Application.Features.EmployeeRequests.Queries.Vacation;

public class VacationRequestListDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeNameAr { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string? BranchName { get; set; }
    public Guid VacationTypeId { get; set; }
    public string? VacationTypeName { get; set; }
    public string? VacationTypeNameAr { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalDays { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Domain.Enums.EmployeeRequestStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateTime? ManagerApprovalDate { get; set; }
    public string? ManagerComments { get; set; }
    public DateTime? HRApprovalDate { get; set; }
    public string? HRComments { get; set; }
    public string? AttachmentUrl { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public string CurrentApprovalLevel { get; set; } = string.Empty;
}
