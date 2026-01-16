using FluentValidation;

namespace HrSystem.Application.Features.Employees.Queries.GetEmployeeById;

public class GetEmployeeByIdQueryValidator : AbstractValidator<GetEmployeeByIdQuery>
{
    public GetEmployeeByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Employee ID is required");
    }
}
