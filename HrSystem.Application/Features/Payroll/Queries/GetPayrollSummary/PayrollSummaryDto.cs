namespace HrSystem.Application.Features.Payroll.Queries.GetPayrollSummary;

public class PayrollSummaryDto
{
    public int EmployeesPaid { get; set; }
    public decimal Gross { get; set; }
    public decimal Deductions { get; set; }
    public decimal NetPayroll { get; set; }
    public int? Year { get; set; }
    public int? Month { get; set; }
}
