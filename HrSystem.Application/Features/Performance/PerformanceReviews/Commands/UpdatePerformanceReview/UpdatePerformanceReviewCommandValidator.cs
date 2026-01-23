using FluentValidation;
using HrSystem.Infrustructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.PerformanceReviews.Commands.UpdatePerformanceReview;

public class UpdatePerformanceReviewCommandValidator : AbstractValidator<UpdatePerformanceReviewCommand>
{
    private readonly ApplicationDbContext _context;

    public UpdatePerformanceReviewCommandValidator(ApplicationDbContext context)
    {
        _context = context;
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Performance review ID is required");

        RuleFor(x => x.ReviewTypeId)
            .NotEmpty().WithMessage("Review type ID is required")
            .MustAsync(ReviewTypeExists).WithMessage("Review type does not exist");

        RuleFor(x => x.StatusId)
            .NotEmpty().WithMessage("Status ID is required")
            .MustAsync(StatusExists).WithMessage("Status does not exist");

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

    private async Task<bool> ReviewTypeExists(Guid reviewTypeId, CancellationToken cancellationToken)
    {
        return await _context.ReviewTypes.AnyAsync(rt => rt.Id == reviewTypeId, cancellationToken);
    }

    private async Task<bool> StatusExists(Guid statusId, CancellationToken cancellationToken)
    {
        return await _context.ReviewStatuses.AnyAsync(rs => rs.Id == statusId, cancellationToken);
    }
}
