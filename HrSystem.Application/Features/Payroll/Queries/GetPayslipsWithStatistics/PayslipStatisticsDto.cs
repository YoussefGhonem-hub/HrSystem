namespace HrSystem.Application.Features.Payroll.Queries.GetPayslipsWithStatistics;

public class PayslipStatisticsDto
{
    public int TotalPayslips { get; set; }
    public int SentPayslips { get; set; }
    public int PendingPayslips { get; set; }
    public decimal AverageNetSalary { get; set; }
    public decimal MedianSalary { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TaxDeductions { get; set; }
    public decimal InsuranceDeductions { get; set; }
    public decimal OtherDeductions { get; set; }
}
