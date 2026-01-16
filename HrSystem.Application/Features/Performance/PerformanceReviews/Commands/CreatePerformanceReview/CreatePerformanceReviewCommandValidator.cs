using FluentValidation;
using HrSystem.Infrustructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.PerformanceReviews.Commands.CreatePerformanceReview;

public class CreatePerformanceReviewCommandValidator : AbstractValidator<CreatePerformanceReviewCommand>
{
    private readonly ApplicationDbContext _context;

    public CreatePerformanceReviewCommandValidator(ApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("Employee ID is required")
            .MustAsync(EmployeeExists).WithMessage("Employee does not exist");

        RuleFor(x => x.ReviewerId)
            .NotEmpty().WithMessage("Reviewer ID is required")
            .MustAsync(ReviewerExists).WithMessage("Reviewer does not exist");

        RuleFor(x => x.ReviewType)
            .NotEmpty().WithMessage("Review type is required")
            .MaximumLength(50).WithMessage("Review type must not exceed 50 characters");

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

    private async Task<bool> EmployeeExists(Guid employeeId, CancellationToken cancellationToken)
    {
        return await _context.Employees.AnyAsync(e => e.Id == employeeId, cancellationToken);
    }

    private async Task<bool> ReviewerExists(Guid reviewerId, CancellationToken cancellationToken)
    {
        return await _context.Employees.AnyAsync(e => e.Id == reviewerId, cancellationToken);
    }
}
