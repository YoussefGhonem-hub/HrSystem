namespace HrSystem.Application.Features.Payroll.Queries.GetMySalaryBreakdown;

public class SalaryBreakdownDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string PeriodLabel { get; set; } = string.Empty; // e.g., "October 2026"

    public List<BreakdownItemDto> Earnings { get; set; } = new();
    public List<BreakdownItemDto> Deductions { get; set; } = new();

    public decimal TotalEarnings { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetSalary { get; set; }
    public bool IsPaid { get; set; }
}

public class BreakdownItemDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
