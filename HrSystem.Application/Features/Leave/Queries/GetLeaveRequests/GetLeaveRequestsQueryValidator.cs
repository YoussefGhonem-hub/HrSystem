using FluentValidation;

namespace HrSystem.Application.Features.Leave.Queries.GetLeaveRequests;

public class GetLeaveRequestsQueryValidator : AbstractValidator<GetLeaveRequestsQuery>
{
    public GetLeaveRequestsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100");

        RuleFor(x => x.StartDateFrom)
            .LessThanOrEqualTo(x => x.StartDateTo)
            .When(x => x.StartDateFrom.HasValue && x.StartDateTo.HasValue)
            .WithMessage("Start date from must be less than or equal to start date to");
    }
}
