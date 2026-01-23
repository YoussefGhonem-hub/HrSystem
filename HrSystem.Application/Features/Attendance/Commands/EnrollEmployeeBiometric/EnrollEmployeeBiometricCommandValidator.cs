using FluentValidation;

namespace HrSystem.Application.Features.Attendance.Commands.EnrollEmployeeBiometric;

public class EnrollEmployeeBiometricCommandValidator : AbstractValidator<EnrollEmployeeBiometricCommand>
{
    public EnrollEmployeeBiometricCommandValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("Employee id is required");

        RuleFor(x => x.TemplateBase64)
            .NotEmpty().WithMessage("Template is required");
    }
}
