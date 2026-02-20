namespace HrSystem.Application.Features.LeaveBalances.Queries.GetLeaveReport;

public class LeaveReportStatisticsDto
{
    public int TotalLeaveRequests { get; set; }
    public int ApprovedRequests { get; set; }
    public int PendingRequests { get; set; }
    public int RejectedRequests { get; set; }
    public decimal TotalDaysUsed { get; set; }
    public decimal AverageDaysPerRequest { get; set; }
    public List<VacationTypeBreakdownDto> BreakdownByVacationType { get; set; } = new();
}

public class VacationTypeBreakdownDto
{
    public Guid VacationTypeId { get; set; }
    public string VacationTypeNameEn { get; set; } = string.Empty;
    public string VacationTypeNameAr { get; set; } = string.Empty;
    public int RequestCount { get; set; }
    public decimal TotalDays { get; set; }
}
