using HrSystem.Application.Common.PaginatedList;

namespace HrSystem.Application.Features.LeaveBalances.Queries.GetLeaveReport;

public class LeaveReportResponseDto
{
    public LeaveReportStatisticsDto Statistics { get; set; } = new();
    public PagedResult<LeaveReportListDto> LeaveRequests { get; set; } = new();
}
