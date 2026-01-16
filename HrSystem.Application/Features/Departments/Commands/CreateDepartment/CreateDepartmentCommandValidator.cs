using FluentValidation;
using HrSystem.Infrustructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Departments.Commands.CreateDepartment;

public class CreateDepartmentCommandValidator : AbstractValidator<CreateDepartmentCommand>
{
    private readonly ApplicationDbContext _context;

    public CreateDepartmentCommandValidator(ApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.NameAr)
            .NotEmpty().WithMessage("Name in Arabic is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.NameEn)
            .NotEmpty().WithMessage("Name in English is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.ManagerId)
            .MustAsync(ManagerExists).WithMessage("Manager does not exist")
            .When(x => x.ManagerId.HasValue);

        RuleFor(x => x.ParentDepartmentId)
            .MustAsync(ParentDepartmentExists).WithMessage("Parent department does not exist")
            .When(x => x.ParentDepartmentId.HasValue);

        RuleFor(x => x.BranchId)
            .MustAsync(BranchExists).WithMessage("Branch does not exist")
            .When(x => x.BranchId.HasValue);
    }

    private async Task<bool> ManagerExists(Guid? managerId, CancellationToken cancellationToken)
    {
        if (!managerId.HasValue) return true;
        return await _context.Employees.AnyAsync(e => e.Id == managerId.Value, cancellationToken);
    }

    private async Task<bool> ParentDepartmentExists(Guid? parentId, CancellationToken cancellationToken)
    {
        if (!parentId.HasValue) return true;
        return await _context.Departments.AnyAsync(d => d.Id == parentId.Value, cancellationToken);
    }

    private async Task<bool> BranchExists(Guid? branchId, CancellationToken cancellationToken)
    {
        if (!branchId.HasValue) return true;
        return await _context.Branches.AnyAsync(b => b.Id == branchId.Value, cancellationToken);
    }
}
