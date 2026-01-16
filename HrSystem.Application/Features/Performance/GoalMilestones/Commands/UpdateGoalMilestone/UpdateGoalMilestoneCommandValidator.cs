using FluentValidation;

namespace HrSystem.Application.Features.Performance.GoalMilestones.Commands.UpdateGoalMilestone;

public class UpdateGoalMilestoneCommandValidator : AbstractValidator<UpdateGoalMilestoneCommand>
{
    public UpdateGoalMilestoneCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Goal milestone ID is required");

        RuleFor(x => x.TitleAr)
            .NotEmpty().WithMessage("Title in Arabic is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.TitleEn)
            .NotEmpty().WithMessage("Title in English is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.DueDate)
            .NotEmpty().WithMessage("Due date is required");

        RuleFor(x => x.CompletionDate)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Completion date cannot be in the future")
            .When(x => x.CompletionDate.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}
