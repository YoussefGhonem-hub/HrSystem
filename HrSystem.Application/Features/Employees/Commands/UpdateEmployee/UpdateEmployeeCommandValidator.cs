using FluentValidation;
using HrSystem.Infrustructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Employees.Commands.UpdateEmployee;

public class UpdateEmployeeCommandValidator : AbstractValidator<UpdateEmployeeCommand>
{
    private readonly ApplicationDbContext _context;

    public UpdateEmployeeCommandValidator(ApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Employee ID is required");

        RuleFor(x => x.Employee.FirstNameAr)
            .NotEmpty().WithMessage("First name in Arabic is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.Employee.LastNameAr)
            .NotEmpty().WithMessage("Last name in Arabic is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

        RuleFor(x => x.Employee.FirstNameEn)
            .NotEmpty().WithMessage("First name in English is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.Employee.LastNameEn)
            .NotEmpty().WithMessage("Last name in English is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

        RuleFor(x => x.Employee.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MustAsync(BeUniqueEmail).WithMessage("Email already exists");

        RuleFor(x => x.Employee.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters");

        RuleFor(x => x.Employee.DepartmentId)
            .NotEmpty().WithMessage("Department is required")
            .MustAsync(DepartmentExists).WithMessage("Department does not exist");

        RuleFor(x => x.Employee.JobTitleId)
            .NotEmpty().WithMessage("Job title is required")
            .MustAsync(JobTitleExists).WithMessage("Job title does not exist");

        RuleFor(x => x.Employee.DirectManagerId)
            .MustAsync(ManagerExists).WithMessage("Direct manager does not exist")
            .MustAsync(NotBeCircular).WithMessage("Cannot set employee as their own manager")
            .When(x => x.Employee.DirectManagerId.HasValue);

        RuleFor(x => x.Employee.BranchId)
            .MustAsync(BranchExists).WithMessage("Branch does not exist")
            .When(x => x.Employee.BranchId.HasValue);
    }

    private async Task<bool> BeUniqueEmail(UpdateEmployeeCommand command, string email, CancellationToken cancellationToken)
    {
        return !await _context.Employees.AnyAsync(
            e => e.Email == email && e.Id != command.Id, 
            cancellationToken);
    }

    private async Task<bool> DepartmentExists(Guid departmentId, CancellationToken cancellationToken)
    {
        return await _context.Departments.AnyAsync(d => d.Id == departmentId, cancellationToken);
    }

    private async Task<bool> JobTitleExists(Guid jobTitleId, CancellationToken cancellationToken)
    {
        return await _context.JobTitles.AnyAsync(j => j.Id == jobTitleId, cancellationToken);
    }

    private async Task<bool> ManagerExists(Guid? managerId, CancellationToken cancellationToken)
    {
        if (!managerId.HasValue) return true;
        return await _context.Employees.AnyAsync(e => e.Id == managerId.Value, cancellationToken);
    }

    private async Task<bool> BranchExists(Guid? branchId, CancellationToken cancellationToken)
    {
        if (!branchId.HasValue) return true;
        return await _context.Branches.AnyAsync(b => b.Id == branchId.Value, cancellationToken);
    }

    private async Task<bool> NotBeCircular(UpdateEmployeeCommand command, Guid? managerId, CancellationToken cancellationToken)
    {
        if (!managerId.HasValue) return true;
        return managerId.Value != command.Id;
    }
}
