namespace HrSystem.Application.Features.Payroll.Loans.Queries.GetLoansList;

public class LoanListDto
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
    public bool IsActive { get; set; }
}
