using FluentValidation;

namespace HrSystem.Application.Features.Departments.Commands.DeleteDepartment;

public class DeleteDepartmentCommandValidator : AbstractValidator<DeleteDepartmentCommand>
{
    public DeleteDepartmentCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Department ID is required");
    }
}
