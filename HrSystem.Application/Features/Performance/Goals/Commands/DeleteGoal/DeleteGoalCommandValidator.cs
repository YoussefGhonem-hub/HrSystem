using FluentValidation;
using HrSystem.Infrustructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.Goals.Commands.DeleteGoal;

public class DeleteGoalCommandValidator : AbstractValidator<DeleteGoalCommand>
{
    private readonly ApplicationDbContext _context;

    public DeleteGoalCommandValidator(ApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Goal ID is required")
            .MustAsync(GoalExists).WithMessage("Goal does not exist");
    }

    private async Task<bool> GoalExists(Guid goalId, CancellationToken cancellationToken)
    {
        return await _context.Goals.AnyAsync(g => g.Id == goalId, cancellationToken);
    }
}
