using FluentValidation;

namespace HrSystem.Application.Features.Attendance.Commands.QuickCheckInOut;

public class QuickCheckInOutCommandValidator : AbstractValidator<QuickCheckInOutCommand>
{
    public QuickCheckInOutCommandValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("EmployeeId is required.");

        RuleFor(x => x.PunchType)
            .IsInEnum().WithMessage("Invalid punch type. Must be CheckIn or CheckOut.");

        RuleFor(x => x.EventDateTime)
            .NotEmpty().WithMessage("EventDateTime is required.");
    }
}
