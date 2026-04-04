namespace HrSystem.Application.Features.Payroll.Commands.ConfigureEmployeePayroll;

public class PayrollBankInfoPayload
{
    public string? BankName { get; set; }
    public string? BankBranch { get; set; }
    public string? AccountNumber { get; set; }
    public string? AccountName { get; set; }
    public string? Iban { get; set; }
    public string? SwiftCode { get; set; }
}

public class PayrollAllowancePayload
{
    public string NameAr { get; set; } = "";
    public string NameEn { get; set; } = "";
    public string? Description { get; set; }
    public bool IsTaxable { get; set; }
    public bool IsSubjectToInsurance { get; set; }
    public decimal Amount { get; set; }
    public bool IsPercentage { get; set; }
    public decimal? PercentageValue { get; set; }
}

public class PayrollDeductionPayload
{
    public string NameAr { get; set; } = "";
    public string NameEn { get; set; } = "";
    public string? Description { get; set; }
    public bool IsRecurring { get; set; }
    public decimal Amount { get; set; }
    public bool IsPercentage { get; set; }
    public decimal? PercentageValue { get; set; }
}
