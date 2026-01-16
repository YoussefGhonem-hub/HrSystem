using FluentValidation;
using HrSystem.Infrustructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Branches.Commands.UpdateBranch;

public class UpdateBranchCommandValidator : AbstractValidator<UpdateBranchCommand>
{
    private readonly ApplicationDbContext _context;

    public UpdateBranchCommandValidator(ApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Branch ID is required");

        RuleFor(x => x.NameAr)
            .NotEmpty().WithMessage("Name in Arabic is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.NameEn)
            .NotEmpty().WithMessage("Name in English is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email format")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.TimeZone)
            .NotEmpty().WithMessage("Time zone is required");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required");

        RuleFor(x => x.BranchManagerId)
            .MustAsync(ManagerExists).WithMessage("Branch manager does not exist")
            .When(x => x.BranchManagerId.HasValue);
    }

    private async Task<bool> ManagerExists(Guid? managerId, CancellationToken cancellationToken)
    {
        if (!managerId.HasValue) return true;
        return await _context.Employees.AnyAsync(e => e.Id == managerId.Value, cancellationToken);
    }
}
