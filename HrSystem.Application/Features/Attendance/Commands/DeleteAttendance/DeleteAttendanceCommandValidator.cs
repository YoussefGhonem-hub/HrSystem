using FluentValidation;

namespace HrSystem.Application.Features.Attendance.Commands.DeleteAttendance;

public class DeleteAttendanceCommandValidator : AbstractValidator<DeleteAttendanceCommand>
{
    public DeleteAttendanceCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Attendance ID is required");
    }
}
