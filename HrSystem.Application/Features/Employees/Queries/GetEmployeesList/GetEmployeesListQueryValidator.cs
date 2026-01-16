using FluentValidation;

namespace HrSystem.Application.Features.Employees.Queries.GetEmployeesList;

public class GetEmployeesListQueryValidator : AbstractValidator<GetEmployeesListQuery>
{
    public GetEmployeesListQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100");

        RuleFor(x => x.SearchTerm)
            .MaximumLength(200).WithMessage("Search term must not exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.SearchTerm));
    }
}
