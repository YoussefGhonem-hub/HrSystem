using FluentValidation;

namespace HrSystem.Application.Features.Performance.PerformanceReviews.Commands.DeletePerformanceReview;

public class DeletePerformanceReviewCommandValidator : AbstractValidator<DeletePerformanceReviewCommand>
{
    public DeletePerformanceReviewCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Performance review ID is required");
    }
}
