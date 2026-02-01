using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using HrSystem.Application.Features.Payroll.Commands.ConfigureEmployeePayroll;
using HrSystem.Infrustructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Employees.Commands.UpdateEmployeePayroll;

public class UpdateEmployeePayrollCommandValidator : AbstractValidator<UpdateEmployeePayrollCommand>
{
    private readonly ApplicationDbContext _context;

    public UpdateEmployeePayrollCommandValidator(ApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("Employee ID is required")
            .MustAsync(EmployeeExists).WithMessage("Employee not found");

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

    private async Task<bool> EmployeeExists(Guid employeeId, CancellationToken cancellationToken)
    {
        return await _context.Employees.AnyAsync(e => e.Id == employeeId, cancellationToken);
    }
}
