using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using HrSystem.Infrustructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Employees.Commands.UpdateEmployeePersonalInfo;

public class UpdateEmployeePersonalInfoCommandValidator : AbstractValidator<UpdateEmployeePersonalInfoCommand>
{
    private readonly ApplicationDbContext _context;

    public UpdateEmployeePersonalInfoCommandValidator(ApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("Employee ID is required")
            .MustAsync(EmployeeExists).WithMessage("Employee not found");

        RuleFor(x => x.FirstNameAr)
            .NotEmpty().WithMessage("First name in Arabic is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.LastNameAr)
            .NotEmpty().WithMessage("Last name in Arabic is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

        RuleFor(x => x.FirstNameEn)
            .NotEmpty().WithMessage("First name in English is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.LastNameEn)
            .NotEmpty().WithMessage("Last name in English is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

        RuleFor(x => x.NationalId)
            .NotEmpty().WithMessage("National ID is required")
            .MaximumLength(20).WithMessage("National ID must not exceed 20 characters")
            .MustAsync(BeUniqueNationalId).WithMessage("National ID already exists");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MustAsync(BeUniqueEmail).WithMessage("Email already exists");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Date of birth is required")
            .Must(BeValidAge).WithMessage("Employee must be at least 18 years old");
    }

    private async Task<bool> EmployeeExists(Guid employeeId, CancellationToken cancellationToken)
    {
        return await _context.Employees.AnyAsync(e => e.Id == employeeId, cancellationToken);
    }

    private async Task<bool> BeUniqueNationalId(UpdateEmployeePersonalInfoCommand command, string nationalId, CancellationToken cancellationToken)
    {
        return !await _context.Employees
            .AnyAsync(e => e.NationalId == nationalId && e.Id != command.EmployeeId, cancellationToken);
    }

    private async Task<bool> BeUniqueEmail(UpdateEmployeePersonalInfoCommand command, string email, CancellationToken cancellationToken)
    {
        return !await _context.Employees
            .AnyAsync(e => e.Email == email && e.Id != command.EmployeeId, cancellationToken);
    }

    private bool BeValidAge(DateTime dateOfBirth)
    {
        var age = DateTime.Today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > DateTime.Today.AddYears(-age)) age--;
        return age >= 18;
    }
}
