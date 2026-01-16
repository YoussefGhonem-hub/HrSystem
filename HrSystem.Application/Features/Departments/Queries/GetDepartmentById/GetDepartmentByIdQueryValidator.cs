using FluentValidation;

namespace HrSystem.Application.Features.Departments.Queries.GetDepartmentById;

public class GetDepartmentByIdQueryValidator : AbstractValidator<GetDepartmentByIdQuery>
{
    public GetDepartmentByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Department ID is required");
    }
}
