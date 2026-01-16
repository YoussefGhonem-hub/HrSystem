using FluentValidation;
using HrSystem.Infrustructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.GoalStatuses.Commands.UpdateGoalStatus;

public class UpdateGoalStatusCommandValidator : AbstractValidator<UpdateGoalStatusCommand>
{
    private readonly ApplicationDbContext _context;

    public UpdateGoalStatusCommandValidator(ApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required");

        RuleFor(x => x.NameAr)
            .NotEmpty().WithMessage("Name in Arabic is required")
            .MaximumLength(100).WithMessage("Name in Arabic must not exceed 100 characters")
            .MustAsync(BeUniqueNameAr).WithMessage("Name in Arabic already exists");

        RuleFor(x => x.NameEn)
            .NotEmpty().WithMessage("Name in English is required")
            .MaximumLength(100).WithMessage("Name in English must not exceed 100 characters")
            .MustAsync(BeUniqueNameEn).WithMessage("Name in English already exists");

        RuleFor(x => x.DescriptionAr)
            .MaximumLength(500).WithMessage("Description in Arabic must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.DescriptionAr));

        RuleFor(x => x.DescriptionEn)
            .MaximumLength(500).WithMessage("Description in English must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.DescriptionEn));

        RuleFor(x => x.DisplayOrder)
            .NotEmpty().WithMessage("Display order is required")
            .GreaterThan(0).WithMessage("Display order must be greater than 0");
    }

    private async Task<bool> BeUniqueNameAr(UpdateGoalStatusCommand command, string nameAr, CancellationToken cancellationToken)
    {
        return !await _context.GoalStatuses.AnyAsync(gs => gs.NameAr == nameAr && gs.Id != command.Id, cancellationToken);
    }

    private async Task<bool> BeUniqueNameEn(UpdateGoalStatusCommand command, string nameEn, CancellationToken cancellationToken)
    {
        return !await _context.GoalStatuses.AnyAsync(gs => gs.NameEn == nameEn && gs.Id != command.Id, cancellationToken);
    }
}
