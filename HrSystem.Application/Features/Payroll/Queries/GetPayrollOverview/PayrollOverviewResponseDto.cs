using HrSystem.Application.Common.PaginatedList;

namespace HrSystem.Application.Features.Payroll.Queries.GetPayrollOverview;

public class PayrollOverviewResponseDto
{
    public PayrollStatisticsDto Statistics { get; set; } = new();
    public PagedResult<PayrollOverviewDto> PayrollData { get; set; } = new();
}
