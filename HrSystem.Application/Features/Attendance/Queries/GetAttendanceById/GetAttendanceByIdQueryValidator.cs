using FluentValidation;

namespace HrSystem.Application.Features.Attendance.Queries.GetAttendanceById;

public class GetAttendanceByIdQueryValidator : AbstractValidator<GetAttendanceByIdQuery>
{
    public GetAttendanceByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Attendance ID is required");
    }
}
