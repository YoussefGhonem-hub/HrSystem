namespace HrSystem.Application.Features.LeaveBalances.Queries.GetLeaveReport;

public class LeaveReportListDto
{
    public Guid RequestId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeNameEn { get; set; } = string.Empty;
    public string EmployeeNameAr { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string VacationTypeNameEn { get; set; } = string.Empty;
    public string VacationTypeNameAr { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal TotalDays { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime RequestedDate { get; set; }
}
