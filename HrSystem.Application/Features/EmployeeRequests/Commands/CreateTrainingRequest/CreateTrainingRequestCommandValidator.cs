using FluentValidation;

namespace HrSystem.Application.Features.EmployeeRequests.Commands.CreateTrainingRequest;

public class CreateTrainingRequestCommandValidator : AbstractValidator<CreateTrainingRequestCommand>
{
    public CreateTrainingRequestCommandValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(250);
        RuleFor(x => x.TrainingName).NotEmpty().MaximumLength(300);
        RuleFor(x => x.TrainingStartDate).NotEmpty();
        RuleFor(x => x.TrainingEndDate).NotEmpty();
        RuleFor(x => x.DurationDays).GreaterThan(0);

        RuleFor(x => x)
            .Must(c => c.TrainingEndDate.Date >= c.TrainingStartDate.Date)
            .WithMessage("Training end date must be >= start date.");

        RuleFor(x => x.TrainingProvider).MaximumLength(200);
        RuleFor(x => x.TrainingLocation).MaximumLength(300);
        RuleFor(x => x.Currency).MaximumLength(10);
        RuleFor(x => x.Objectives).MaximumLength(2000);
        RuleFor(x => x.ExpectedOutcome).MaximumLength(2000);

        RuleFor(x => x.EstimatedCost)
            .GreaterThanOrEqualTo(0)
            .When(x => x.EstimatedCost.HasValue);
    }
}
