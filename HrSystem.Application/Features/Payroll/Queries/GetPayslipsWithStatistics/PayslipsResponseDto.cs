using HrSystem.Application.Common.PaginatedList;

namespace HrSystem.Application.Features.Payroll.Queries.GetPayslipsWithStatistics;

public class PayslipsResponseDto
{
    public PayslipStatisticsDto Statistics { get; set; } = new();
    public PagedResult<PayslipListDto> PayslipsData { get; set; } = new();
}
