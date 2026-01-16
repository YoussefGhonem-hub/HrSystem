using FluentValidation;
using HrSystem.Infrustructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Branches.Commands.CreateBranch;

public class CreateBranchCommandValidator : AbstractValidator<CreateBranchCommand>
{
    private readonly ApplicationDbContext _context;

    public CreateBranchCommandValidator(ApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.Branch.NameAr)
            .NotEmpty().WithMessage("Name in Arabic is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.Branch.NameEn)
            .NotEmpty().WithMessage("Name in English is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.Branch.Code)
            .NotEmpty().WithMessage("Branch code is required")
            .MaximumLength(20).WithMessage("Branch code must not exceed 20 characters")
            .MustAsync(BeUniqueBranchCode).WithMessage("Branch code already exists");

        RuleFor(x => x.Branch.Email)
            .EmailAddress().WithMessage("Invalid email format")
            .When(x => !string.IsNullOrEmpty(x.Branch.Email));

        RuleFor(x => x.Branch.TimeZone)
            .NotEmpty().WithMessage("Time zone is required");

        RuleFor(x => x.Branch.Currency)
            .NotEmpty().WithMessage("Currency is required");

        RuleFor(x => x.Branch.BranchManagerId)
            .MustAsync(ManagerExists).WithMessage("Branch manager does not exist")
            .When(x => x.Branch.BranchManagerId.HasValue);
    }

    private async Task<bool> BeUniqueBranchCode(string code, CancellationToken cancellationToken)
    {
        return !await _context.Branches.AnyAsync(b => b.Code == code, cancellationToken);
    }

    private async Task<bool> ManagerExists(Guid? managerId, CancellationToken cancellationToken)
    {
        if (!managerId.HasValue) return true;
        return await _context.Employees.AnyAsync(e => e.Id == managerId.Value, cancellationToken);
    }
}
