using FluentValidation;
using HrSystem.Infrustructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.Feedbacks.Commands.CreateFeedback;

public class CreateFeedbackCommandValidator : AbstractValidator<CreateFeedbackCommand>
{
    private readonly ApplicationDbContext _context;

    public CreateFeedbackCommandValidator(ApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.PerformanceReviewId)
            .NotEmpty().WithMessage("Performance review ID is required")
            .MustAsync(PerformanceReviewExists).WithMessage("Performance review does not exist");

        RuleFor(x => x.ProvidedBy)
            .NotEmpty().WithMessage("Provider ID is required")
            .MustAsync(EmployeeExists).WithMessage("Provider employee does not exist");

        RuleFor(x => x.FeedbackType)
            .NotEmpty().WithMessage("Feedback type is required")
            .Must(BeValidFeedbackType).WithMessage("Invalid feedback type. Valid values: Manager, Peer, Self, Subordinate");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5");

        RuleFor(x => x.Comments)
            .MaximumLength(2000).WithMessage("Comments must not exceed 2000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Comments));
    }

    private bool BeValidFeedbackType(string feedbackType)
    {
        var validTypes = new[] { "Manager", "Peer", "Self", "Subordinate" };
        return validTypes.Contains(feedbackType);
    }

    private async Task<bool> PerformanceReviewExists(Guid performanceReviewId, CancellationToken cancellationToken)
    {
        return await _context.PerformanceReviews.AnyAsync(pr => pr.Id == performanceReviewId, cancellationToken);
    }

    private async Task<bool> EmployeeExists(Guid employeeId, CancellationToken cancellationToken)
    {
        return await _context.Employees.AnyAsync(e => e.Id == employeeId, cancellationToken);
    }
}
