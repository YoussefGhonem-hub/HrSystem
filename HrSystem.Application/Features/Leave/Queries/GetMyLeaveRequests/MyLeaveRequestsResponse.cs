using HrSystem.Application.Common.PaginatedList;
using HrSystem.Application.Features.Leave.Queries.GetLeaveRequests;
using System.Collections.Generic;

namespace HrSystem.Application.Features.Leave.Queries.GetMyLeaveRequests;

public class MyLeaveRequestsResponse
{
    public required PagedResult<LeaveRequestDto> Requests { get; set; }
    public IReadOnlyList<LeaveStatusStatisticDto> Statistics { get; set; } = Array.Empty<LeaveStatusStatisticDto>();
}

public class LeaveStatusStatisticDto
{
    public Guid StatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string StatusNameAr { get; set; } = string.Empty;
    public int Total { get; set; }
}
