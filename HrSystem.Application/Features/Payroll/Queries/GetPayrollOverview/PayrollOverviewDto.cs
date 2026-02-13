namespace HrSystem.Application.Features.Payroll.Queries.GetPayrollOverview;

public class PayrollOverviewDto
{
    public Guid DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetSalary { get; set; }
    public string Status { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
}
