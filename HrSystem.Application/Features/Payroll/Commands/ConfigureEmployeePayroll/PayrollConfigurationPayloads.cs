namespace HrSystem.Application.Features.Payroll.Commands.ConfigureEmployeePayroll;

public record PayrollBankInfoPayload(
    string? BankName,
    string? BankBranch,
    string? AccountNumber,
    string? Iban,
    string? SwiftCode
);

public record PayrollAllowancePayload(
    Guid AllowanceTypeId,
    decimal Amount,
    bool IsPercentage,
    decimal? PercentageValue
);

public record PayrollDeductionPayload(
    Guid DeductionTypeId,
    decimal Amount,
    bool IsPercentage,
    decimal? PercentageValue
);
