using FluentValidation;
using HrSystem.Infrustructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.GoalMilestones.Commands.CreateGoalMilestone;

public class CreateGoalMilestoneCommandValidator : AbstractValidator<CreateGoalMilestoneCommand>
{
    private readonly ApplicationDbContext _context;

    public CreateGoalMilestoneCommandValidator(ApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.GoalId)
            .NotEmpty().WithMessage("Goal ID is required")
            .MustAsync(GoalExists).WithMessage("Goal does not exist");

        RuleFor(x => x.TitleAr)
            .NotEmpty().WithMessage("Title in Arabic is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.TitleEn)
            .NotEmpty().WithMessage("Title in English is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.DueDate)
            .NotEmpty().WithMessage("Due date is required")
            .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Due date cannot be in the past");

        RuleFor(x => x.CompletionDate)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Completion date cannot be in the future")
            .When(x => x.CompletionDate.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }

    private async Task<bool> GoalExists(Guid goalId, CancellationToken cancellationToken)
    {
        return await _context.Goals.AnyAsync(g => g.Id == goalId, cancellationToken);
    }
}
