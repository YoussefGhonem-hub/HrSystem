using FluentValidation;

namespace HrSystem.Application.Features.Performance.PerformanceReviews.Queries.GetPerformanceReviewsList;

public class GetPerformanceReviewsListQueryValidator : AbstractValidator<GetPerformanceReviewsListQuery>
{
    public GetPerformanceReviewsListQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100");

        RuleFor(x => x.ReviewDateTo)
            .GreaterThanOrEqualTo(x => x.ReviewDateFrom)
            .WithMessage("Review date to must be greater than or equal to review date from")
            .When(x => x.ReviewDateFrom.HasValue && x.ReviewDateTo.HasValue);
    }
}
