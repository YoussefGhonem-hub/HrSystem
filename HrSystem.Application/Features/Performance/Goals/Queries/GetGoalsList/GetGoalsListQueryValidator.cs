using FluentValidation;

namespace HrSystem.Application.Features.Performance.Goals.Queries.GetGoalsList;

public class GetGoalsListQueryValidator : AbstractValidator<GetGoalsListQuery>
{
    public GetGoalsListQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100");

        RuleFor(x => x.Status)
            .Must(s => string.IsNullOrEmpty(s) || new[] { "NotStarted", "InProgress", "Completed", "Cancelled" }.Contains(s))
            .WithMessage("Status must be NotStarted, InProgress, Completed, or Cancelled");

        RuleFor(x => x.Priority)
            .Must(p => string.IsNullOrEmpty(p) || new[] { "Low", "Medium", "High" }.Contains(p))
            .WithMessage("Priority must be Low, Medium, or High");

        RuleFor(x => x.StartDateTo)
            .GreaterThanOrEqualTo(x => x.StartDateFrom)
            .When(x => x.StartDateFrom.HasValue && x.StartDateTo.HasValue)
            .WithMessage("Start date to must be after or equal to start date from");

        RuleFor(x => x.TargetDateTo)
            .GreaterThanOrEqualTo(x => x.TargetDateFrom)
            .When(x => x.TargetDateFrom.HasValue && x.TargetDateTo.HasValue)
            .WithMessage("Target date to must be after or equal to target date from");
    }
}
