namespace HrSystem.Application.Features.Payroll.Commands.ConfigureEmployeePayroll;

public record PayrollBankInfoPayload(
    string? BankName,
    string? BankBranch,
    string? AccountNumber,
    string? Iban,
    string? SwiftCode
);

public record PayrollAllowancePayload(
    string NameAr,
    string NameEn,
    string? Description,
    bool IsTaxable,
    bool IsSubjectToInsurance,
    decimal Amount,
    bool IsPercentage,
    decimal? PercentageValue
);

public record PayrollDeductionPayload(
    string NameAr,
    string NameEn,
    string? Description,
    bool IsRecurring,
    decimal Amount,
    bool IsPercentage,
    decimal? PercentageValue
);
