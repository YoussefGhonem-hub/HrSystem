using FluentValidation;

namespace HrSystem.Application.Features.Performance.PerformanceReviews.Commands.UpdatePerformanceReview;

public class UpdatePerformanceReviewCommandValidator : AbstractValidator<UpdatePerformanceReviewCommand>
{
    public UpdatePerformanceReviewCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Performance review ID is required");

        RuleFor(x => x.ReviewType)
            .NotEmpty().WithMessage("Review type is required")
            .MaximumLength(50).WithMessage("Review type must not exceed 50 characters");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .MaximumLength(50).WithMessage("Status must not exceed 50 characters");

        RuleFor(x => x.ReviewPeriodStart)
            .NotEmpty().WithMessage("Review period start is required");

        RuleFor(x => x.ReviewPeriodEnd)
            .NotEmpty().WithMessage("Review period end is required")
            .GreaterThan(x => x.ReviewPeriodStart).WithMessage("Review period end must be after start");

        RuleFor(x => x.ReviewDate)
            .NotEmpty().WithMessage("Review date is required");

        RuleFor(x => x.OverallRating)
            .GreaterThanOrEqualTo(0).WithMessage("Overall rating must be at least 0")
            .LessThanOrEqualTo(5).WithMessage("Overall rating must not exceed 5");
    }
}
