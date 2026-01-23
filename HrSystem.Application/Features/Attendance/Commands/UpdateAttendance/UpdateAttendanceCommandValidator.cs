using FluentValidation;

namespace HrSystem.Application.Features.Attendance.Commands.UpdateAttendance;

public class UpdateAttendanceCommandValidator : AbstractValidator<UpdateAttendanceCommand>
{
    public UpdateAttendanceCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Attendance ID is required");

        RuleFor(x => x.StatusId)
            .NotEmpty().WithMessage("Status is required");

        RuleFor(x => x.CheckOutTime)
            .GreaterThan(x => x.CheckInTime)
            .WithMessage("Check-out time must be after check-in time")
            .When(x => x.CheckInTime.HasValue && x.CheckOutTime.HasValue);
    }
}
