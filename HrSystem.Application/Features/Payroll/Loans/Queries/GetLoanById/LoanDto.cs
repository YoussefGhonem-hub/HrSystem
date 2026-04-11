namespace HrSystem.Application.Features.Payroll.Loans.Queries.GetLoanById;

/// <summary>
/// Represents a single payroll month in which a loan installment was actually deducted.
/// Used by the frontend to show accurate 'Paid' status per installment row.
/// </summary>
public class PaidPaymentPeriodDto
{
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal Amount { get; set; }
}

public class LoanDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeNameEn { get; set; } = string.Empty;
    public string EmployeeNameAr { get; set; } = string.Empty;
    public string LoanName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public decimal MonthlyDeduction { get; set; }
    public int InstallmentMonths { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }

    /// <summary>
    /// The payroll months in which this loan's installment was actually deducted from a payslip.
    /// Populated from PayslipDeductions.LoanId join. Empty for old payslips created before
    /// the LoanId column was added (those fall back to count-based schedule on the frontend).
    /// </summary>
    public List<PaidPaymentPeriodDto> PaidPaymentPeriods { get; set; } = new();
}
