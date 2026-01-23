using FluentValidation;

namespace HrSystem.Application.Features.Employees.Commands.AddEmployeeSalary;

public class AddEmployeeSalaryCommandValidator : AbstractValidator<AddEmployeeSalaryCommand>
{
    public AddEmployeeSalaryCommandValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("Employee id is required");

        RuleFor(x => x.BasicSalary)
            .GreaterThanOrEqualTo(0).WithMessage("Basic salary cannot be negative");

        RuleFor(x => x.EffectiveDate)
            .NotEmpty().WithMessage("Effective date is required");
    }
}
