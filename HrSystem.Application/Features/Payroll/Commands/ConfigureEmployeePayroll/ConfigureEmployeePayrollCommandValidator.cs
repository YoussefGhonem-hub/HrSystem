using FluentValidation;

namespace HrSystem.Application.Features.Payroll.Commands.ConfigureEmployeePayroll;

public class ConfigureEmployeePayrollCommandValidator : AbstractValidator<ConfigureEmployeePayrollCommand>
{
    public ConfigureEmployeePayrollCommandValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty();

        RuleFor(x => x.BasicSalary)
            .GreaterThanOrEqualTo(0m);

        RuleFor(x => x.EffectiveDate)
            .NotEmpty();

        RuleFor(x => x.Currency)
            .MaximumLength(10)
            .When(x => !string.IsNullOrWhiteSpace(x.Currency));

        RuleFor(x => x.PaymentMethod)
            .MaximumLength(64)
            .When(x => !string.IsNullOrWhiteSpace(x.PaymentMethod));

        RuleFor(x => x.SocialInsuranceEmployeeRate)
            .InclusiveBetween(0, 100)
            .When(x => x.SocialInsuranceEmployeeRate.HasValue);

        RuleFor(x => x.SocialInsuranceEmployerRate)
            .InclusiveBetween(0, 100)
            .When(x => x.SocialInsuranceEmployerRate.HasValue);

        When(x => x.BankInfo is not null, () =>
        {
            RuleFor(x => x.BankInfo!)
                .SetValidator(new PayrollBankInfoPayloadValidator());
        });

        RuleForEach(x => x.Allowances)
            .SetValidator(new PayrollAllowancePayloadValidator());

        RuleForEach(x => x.Deductions)
            .SetValidator(new PayrollDeductionPayloadValidator());
    }
}

public class PayrollBankInfoPayloadValidator : AbstractValidator<PayrollBankInfoPayload>
{
    public PayrollBankInfoPayloadValidator()
    {
        RuleFor(x => x.BankName).MaximumLength(128);
        RuleFor(x => x.BankBranch).MaximumLength(128);
        RuleFor(x => x.AccountNumber).MaximumLength(64);
        RuleFor(x => x.Iban).MaximumLength(64);
        RuleFor(x => x.SwiftCode).MaximumLength(16);
    }
}

public class PayrollAllowancePayloadValidator : AbstractValidator<PayrollAllowancePayload>
{
    public PayrollAllowancePayloadValidator()
    {
        RuleFor(x => x.AllowanceTypeId)
            .NotEmpty();

        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0m)
            .When(x => !x.IsPercentage);

        RuleFor(x => x.PercentageValue)
            .NotNull().WithMessage("Percentage value is required when allowance is percentage-based")
            .InclusiveBetween(0.01m, 100m)
            .When(x => x.IsPercentage);
    }
}

public class PayrollDeductionPayloadValidator : AbstractValidator<PayrollDeductionPayload>
{
    public PayrollDeductionPayloadValidator()
    {
        RuleFor(x => x.DeductionTypeId)
            .NotEmpty();

        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0m)
            .When(x => !x.IsPercentage);

        RuleFor(x => x.PercentageValue)
            .NotNull().WithMessage("Percentage value is required when deduction is percentage-based")
            .InclusiveBetween(0.01m, 100m)
            .When(x => x.IsPercentage);
    }
}
