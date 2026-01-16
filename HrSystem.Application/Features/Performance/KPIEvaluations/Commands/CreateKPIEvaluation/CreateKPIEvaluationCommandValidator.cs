using FluentValidation;
using HrSystem.Infrustructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.KPIEvaluations.Commands.CreateKPIEvaluation;

public class CreateKPIEvaluationCommandValidator : AbstractValidator<CreateKPIEvaluationCommand>
{
    private readonly ApplicationDbContext _context;

    public CreateKPIEvaluationCommandValidator(ApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.PerformanceReviewId)
            .NotEmpty().WithMessage("Performance review ID is required")
            .MustAsync(PerformanceReviewExists).WithMessage("Performance review does not exist");

        RuleFor(x => x.KPIId)
            .NotEmpty().WithMessage("KPI ID is required")
            .MustAsync(KPIExists).WithMessage("KPI does not exist");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5");

        RuleFor(x => x.WeightedScore)
            .GreaterThanOrEqualTo(0).WithMessage("Weighted score must be non-negative");

        RuleFor(x => x.Comments)
            .MaximumLength(2000).WithMessage("Comments must not exceed 2000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Comments));

        RuleFor(x => x.Evidence)
            .MaximumLength(1000).WithMessage("Evidence must not exceed 1000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Evidence));
    }

    private async Task<bool> PerformanceReviewExists(Guid performanceReviewId, CancellationToken cancellationToken)
    {
        return await _context.PerformanceReviews.AnyAsync(pr => pr.Id == performanceReviewId, cancellationToken);
    }

    private async Task<bool> KPIExists(Guid kpiId, CancellationToken cancellationToken)
    {
        return await _context.KPIs.AnyAsync(k => k.Id == kpiId, cancellationToken);
    }
}
