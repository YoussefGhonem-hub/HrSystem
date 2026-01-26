using HrSystem.Application.Features.Leave.Queries.GetLeaveRequests;

namespace HrSystem.Application.Features.Leave.Queries.Manager.GetManagerLeaveOverview;

public class ManagerLeaveOverviewDto
{
    public List<LeaveRequestDto> MyRequests { get; set; } = new();
    public List<LeaveRequestDto> PendingApprovals { get; set; } = new();
}
