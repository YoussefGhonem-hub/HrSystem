using FluentValidation;
using HrSystem.Infrustructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Departments.Commands.UpdateDepartment;

public class UpdateDepartmentCommandValidator : AbstractValidator<UpdateDepartmentCommand>
{
    private readonly ApplicationDbContext _context;

    public UpdateDepartmentCommandValidator(ApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Department ID is required");

        RuleFor(x => x.Department.NameAr)
            .NotEmpty().WithMessage("Name in Arabic is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.Department.NameEn)
            .NotEmpty().WithMessage("Name in English is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.Department.ManagerId)
            .MustAsync(ManagerExists).WithMessage("Manager does not exist")
            .When(x => x.Department.ManagerId.HasValue);

        RuleFor(x => x.Department.ParentDepartmentId)
            .MustAsync(ParentDepartmentExists).WithMessage("Parent department does not exist")
            .MustAsync(NotBeCircular).WithMessage("Cannot set department as its own parent")
            .When(x => x.Department.ParentDepartmentId.HasValue);

        RuleFor(x => x.Department.BranchId)
            .MustAsync(BranchExists).WithMessage("Branch does not exist")
            .When(x => x.Department.BranchId.HasValue);
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

    private async Task<bool> NotBeCircular(UpdateDepartmentCommand command, Guid? parentId, CancellationToken cancellationToken)
    {
        if (!parentId.HasValue) return true;
        return parentId.Value != command.Id;
    }
}
