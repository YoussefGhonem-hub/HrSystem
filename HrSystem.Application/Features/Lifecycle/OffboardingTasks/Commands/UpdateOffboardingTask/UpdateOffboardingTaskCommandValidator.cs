using FluentValidation;
using HrSystem.Infrustructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Lifecycle.OffboardingTasks.Commands.UpdateOffboardingTask;

public class UpdateOffboardingTaskCommandValidator : AbstractValidator<UpdateOffboardingTaskCommand>
{
    private readonly ApplicationDbContext _context;

    public UpdateOffboardingTaskCommandValidator(ApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Task ID is required");

        RuleFor(x => x.TaskNameAr)
            .NotEmpty().WithMessage("Task name in Arabic is required")
            .MaximumLength(200).WithMessage("Task name must not exceed 200 characters");

        RuleFor(x => x.TaskNameEn)
            .NotEmpty().WithMessage("Task name in English is required")
            .MaximumLength(200).WithMessage("Task name must not exceed 200 characters");

        RuleFor(x => x.Sequence)
            .GreaterThan(0).WithMessage("Sequence must be greater than 0");

        RuleFor(x => x.DueDate)
            .NotEmpty().WithMessage("Due date is required");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required")
            .MaximumLength(100).WithMessage("Category must not exceed 100 characters");

        RuleFor(x => x.AssignedTo)
            .MustAsync(AssignedToExists).WithMessage("Assigned person does not exist")
            .When(x => x.AssignedTo.HasValue);
    }

    private async Task<bool> AssignedToExists(Guid? assignedTo, CancellationToken cancellationToken)
    {
        if (!assignedTo.HasValue) return true;
        return await _context.Employees.AnyAsync(e => e.Id == assignedTo.Value, cancellationToken);
    }
}
