using FluentValidation;

namespace HrSystem.Application.Features.Attendance.Queries.GetAttendancesList;

public class GetAttendancesListQueryValidator : AbstractValidator<GetAttendancesListQuery>
{
    public GetAttendancesListQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100");

        RuleFor(x => x.ToDate)
            .GreaterThanOrEqualTo(x => x.FromDate)
            .WithMessage("To date must be greater than or equal to from date")
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue);
    }
}
