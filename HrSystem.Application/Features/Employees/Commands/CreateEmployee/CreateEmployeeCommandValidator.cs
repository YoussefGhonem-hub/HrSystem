using FluentValidation;
using HrSystem.Infrustructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Employees.Commands.CreateEmployee;

public class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
{
    private readonly ApplicationDbContext _context;

    public CreateEmployeeCommandValidator(ApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.Employee.EmployeeCode)
            .NotEmpty().WithMessage("Employee code is required")
            .MaximumLength(50).WithMessage("Employee code must not exceed 50 characters")
            .MustAsync(BeUniqueEmployeeCode).WithMessage("Employee code already exists");

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

        RuleFor(x => x.Employee.NationalId)
            .NotEmpty().WithMessage("National ID is required")
            .MaximumLength(20).WithMessage("National ID must not exceed 20 characters")
            .MustAsync(BeUniqueNationalId).WithMessage("National ID already exists");

        RuleFor(x => x.Employee.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MustAsync(BeUniqueEmail).WithMessage("Email already exists");

        RuleFor(x => x.Employee.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters");

        RuleFor(x => x.Employee.DateOfBirth)
            .NotEmpty().WithMessage("Date of birth is required")
            .Must(BeValidAge).WithMessage("Employee must be at least 18 years old");

        RuleFor(x => x.Employee.DepartmentId)
            .NotEmpty().WithMessage("Department is required")
            .MustAsync(DepartmentExists).WithMessage("Department does not exist");

        RuleFor(x => x.Employee.JobTitleId)
            .NotEmpty().WithMessage("Job title is required")
            .MustAsync(JobTitleExists).WithMessage("Job title does not exist");

        RuleFor(x => x.Employee.DirectManagerId)
            .MustAsync(ManagerExists).WithMessage("Direct manager does not exist")
            .When(x => x.Employee.DirectManagerId.HasValue);

        RuleFor(x => x.Employee.BranchId)
            .MustAsync(BranchExists).WithMessage("Branch does not exist")
            .When(x => x.Employee.BranchId.HasValue);

        RuleFor(x => x.Employee.HiringDate)
            .NotEmpty().WithMessage("Hiring date is required")
            .LessThanOrEqualTo(DateTime.Today).WithMessage("Hiring date cannot be in the future");

        RuleFor(x => x.Employee.ProbationPeriodMonths)
            .InclusiveBetween(0, 12).WithMessage("Probation period must be between 0 and 12 months");
    }

    private async Task<bool> BeUniqueEmployeeCode(string employeeCode, CancellationToken cancellationToken)
    {
        return !await _context.Employees.AnyAsync(e => e.EmployeeCode == employeeCode, cancellationToken);
    }

    private async Task<bool> BeUniqueNationalId(string nationalId, CancellationToken cancellationToken)
    {
        return !await _context.Employees.AnyAsync(e => e.NationalId == nationalId, cancellationToken);
    }

    private async Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
    {
        return !await _context.Employees.AnyAsync(e => e.Email == email, cancellationToken);
    }

    private bool BeValidAge(DateTime dateOfBirth)
    {
        var age = DateTime.Today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > DateTime.Today.AddYears(-age)) age--;
        return age >= 18;
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
}
