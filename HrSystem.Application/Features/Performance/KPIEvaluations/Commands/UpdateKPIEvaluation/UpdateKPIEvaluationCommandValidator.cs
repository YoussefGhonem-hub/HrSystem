using FluentValidation;

namespace HrSystem.Application.Features.Performance.KPIEvaluations.Commands.UpdateKPIEvaluation;

public class UpdateKPIEvaluationCommandValidator : AbstractValidator<UpdateKPIEvaluationCommand>
{
    public UpdateKPIEvaluationCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("KPI evaluation ID is required");

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
}
