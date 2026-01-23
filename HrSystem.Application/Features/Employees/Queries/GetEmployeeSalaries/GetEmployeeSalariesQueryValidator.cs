using FluentValidation;

namespace HrSystem.Application.Features.Employees.Queries.GetEmployeeSalaries;

public class GetEmployeeSalariesQueryValidator : AbstractValidator<GetEmployeeSalariesQuery>
{
    public GetEmployeeSalariesQueryValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("Employee id is required");
    }
}
