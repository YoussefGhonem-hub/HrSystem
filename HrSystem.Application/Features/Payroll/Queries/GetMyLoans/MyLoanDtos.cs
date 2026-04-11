namespace HrSystem.Application.Features.Payroll.Queries.GetMyLoans;

/// <summary>
/// Represents a single payroll month in which a loan installment was actually deducted.
/// </summary>
public class PaidPaymentPeriodDto
{
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal Amount { get; set; }
}

public class MyLoanSummaryDto
{
    public Guid LoanId { get; set; }
    public string LoanName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public decimal MonthlyDeduction { get; set; }
    public int InstallmentMonths { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
}

public class MyLoanDetailsDto : MyLoanSummaryDto
{
    public string? Notes { get; set; }

    /// <summary>
    /// The payroll months in which this loan's installment was actually deducted from a payslip.
    /// Empty for payslips created before the LoanId tracking column was added.
    /// </summary>
    public List<PaidPaymentPeriodDto> PaidPaymentPeriods { get; set; } = new();
}

