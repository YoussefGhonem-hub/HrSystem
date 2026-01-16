using FluentValidation;
using HrSystem.Infrustructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.Goals.Commands.CreateGoal;

public class CreateGoalCommandValidator : AbstractValidator<CreateGoalCommand>
{
    private readonly ApplicationDbContext _context;

    public CreateGoalCommandValidator(ApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("Employee ID is required")
            .MustAsync(EmployeeExists).WithMessage("Employee does not exist");

        RuleFor(x => x.TitleAr)
            .NotEmpty().WithMessage("Arabic title is required")
            .MaximumLength(200).WithMessage("Arabic title must not exceed 200 characters");

        RuleFor(x => x.TitleEn)
            .NotEmpty().WithMessage("English title is required")
            .MaximumLength(200).WithMessage("English title must not exceed 200 characters");

        RuleFor(x => x.DescriptionAr)
            .MaximumLength(1000).WithMessage("Arabic description must not exceed 1000 characters");

        RuleFor(x => x.DescriptionEn)
            .MaximumLength(1000).WithMessage("English description must not exceed 1000 characters");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required");

        RuleFor(x => x.TargetDate)
            .NotEmpty().WithMessage("Target date is required")
            .GreaterThan(x => x.StartDate).WithMessage("Target date must be after start date");

        RuleFor(x => x.StatusId)
            .NotEmpty().WithMessage("Status ID is required")
            .MustAsync(StatusExists).WithMessage("Status does not exist");

        RuleFor(x => x.PriorityId)
            .NotEmpty().WithMessage("Priority ID is required")
            .MustAsync(PriorityExists).WithMessage("Priority does not exist");

        RuleFor(x => x.AssignedBy)
            .MustAsync(async (assignedBy, cancellationToken) =>
            {
                if (!assignedBy.HasValue) return true;
                return await _context.Employees.AnyAsync(e => e.Id == assignedBy.Value, cancellationToken);
            }).WithMessage("Assigned by employee does not exist");
    }

    private async Task<bool> EmployeeExists(Guid employeeId, CancellationToken cancellationToken)
    {
        return await _context.Employees.AnyAsync(e => e.Id == employeeId, cancellationToken);
    }

    private async Task<bool> StatusExists(Guid statusId, CancellationToken cancellationToken)
    {
        return await _context.GoalStatuses.AnyAsync(gs => gs.Id == statusId, cancellationToken);
    }

    private async Task<bool> PriorityExists(Guid priorityId, CancellationToken cancellationToken)
    {
        return await _context.GoalPriorities.AnyAsync(gp => gp.Id == priorityId, cancellationToken);
    }
}
