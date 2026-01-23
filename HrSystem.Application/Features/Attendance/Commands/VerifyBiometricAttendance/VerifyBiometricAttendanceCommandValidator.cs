using FluentValidation;

namespace HrSystem.Application.Features.Attendance.Commands.VerifyBiometricAttendance;

public class VerifyBiometricAttendanceCommandValidator : AbstractValidator<VerifyBiometricAttendanceCommand>
{
    public VerifyBiometricAttendanceCommandValidator()
    {
        RuleFor(x => x.TemplateBase64)
            .NotEmpty().WithMessage("Template is required");

        RuleFor(x => x.EventTime)
            .NotEmpty().WithMessage("Event time is required");
    }
}
