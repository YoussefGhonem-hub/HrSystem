using FluentValidation;

namespace HrSystem.Application.Features.Performance.PerformanceReviews.Queries.GetPerformanceReviewById;

public class GetPerformanceReviewByIdQueryValidator : AbstractValidator<GetPerformanceReviewByIdQuery>
{
    public GetPerformanceReviewByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Performance review ID is required");
    }
}
