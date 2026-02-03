namespace HrSystem.Application.Features.Payroll.Commands.ConfigureEmployeePayroll;

public class EmployeePayrollConfigurationDto
{
    public Guid SalaryId { get; set; }
    public Guid EmployeeId { get; set; }
    public decimal BasicSalary { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsCurrent { get; set; }
    public string Currency { get; set; } = string.Empty;
    public bool IncludeSocialInsurance { get; set; }
    public decimal? SocialInsuranceEmployeeRate { get; set; }
    public decimal? SocialInsuranceEmployerRate { get; set; }
    public string? PaymentMethod { get; set; }
    public PayrollBankInfoDto? BankInfo { get; set; }
    public string? Notes { get; set; }
    public List<PayrollAllowanceDto> Allowances { get; set; } = new();
    public List<PayrollDeductionDto> Deductions { get; set; } = new();
}

public class PayrollBankInfoDto
{
    public string? BankName { get; set; }
    public string? BankBranch { get; set; }
    public string? AccountNumber { get; set; }
    public string? Iban { get; set; }
    public string? SwiftCode { get; set; }
}

public class PayrollAllowanceDto
{
    public Guid Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsTaxable { get; set; }
    public bool IsSubjectToInsurance { get; set; }
    public decimal Amount { get; set; }
    public bool IsPercentage { get; set; }
    public decimal? PercentageValue { get; set; }
}

public class PayrollDeductionDto
{
    public Guid Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsRecurring { get; set; }
    public decimal Amount { get; set; }
    public bool IsPercentage { get; set; }
    public decimal? PercentageValue { get; set; }
}
