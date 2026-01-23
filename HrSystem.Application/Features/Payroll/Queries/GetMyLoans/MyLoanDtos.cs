namespace HrSystem.Application.Features.Payroll.Queries.GetMyLoans;

public class MyLoanSummaryDto
{
    public Guid LoanId { get; set; }
    public string LoanName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public decimal MonthlyDeduction { get; set; }
    public int InstallmentMonths { get; set; }
    public bool IsActive { get; set; }
}

public class MyLoanDetailsDto : MyLoanSummaryDto
{
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Notes { get; set; }
}
