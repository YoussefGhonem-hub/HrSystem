namespace HrSystem.Application.Features.Payroll.Queries.GetMySalarySummary;

public class MySalarySummaryDto
{
    public Guid PayslipId { get; set; }
    public Guid PayrollCycleId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal NetSalary { get; set; }
    public DateTime? GeneratedDate { get; set; }
    public bool IsPaid { get; set; }
    public DateTime? PaidDate { get; set; }
}
