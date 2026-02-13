namespace HrSystem.Application.Features.Payroll.Queries.GetPayrollOverview;

public class PayrollStatisticsDto
{
    public decimal TotalMonthlyPayroll { get; set; }
    public decimal LastMonthPayroll { get; set; }
    public decimal PayrollChangePercentage { get; set; }
    public int TotalEmployees { get; set; }
    public int FullTimeEmployees { get; set; }
    public int ContractEmployees { get; set; }
    public int PendingPayrollActions { get; set; }
}
