using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using HrSystem.Infrustructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Employees.Commands.UpdateEmployeeJobInfo;

public class UpdateEmployeeJobInfoCommandValidator : AbstractValidator<UpdateEmployeeJobInfoCommand>
{
    private readonly ApplicationDbContext _context;

    public UpdateEmployeeJobInfoCommandValidator(ApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("Employee ID is required")
            .MustAsync(EmployeeExists).WithMessage("Employee not found");

        RuleFor(x => x.DepartmentId)
            .NotEmpty().WithMessage("Department is required")
            .MustAsync(DepartmentExists).WithMessage("Department does not exist");

        RuleFor(x => x.JobTitleId)
            .NotEmpty().WithMessage("Job title is required")
            .MustAsync(JobTitleExists).WithMessage("Job title does not exist");

        RuleFor(x => x.DirectManagerId)
            .MustAsync(ManagerExists).WithMessage("Direct manager does not exist")
            .When(x => x.DirectManagerId.HasValue);

        RuleFor(x => x.BranchId)
            .MustAsync(BranchExists).WithMessage("Branch does not exist")
            .When(x => x.BranchId.HasValue);

        RuleFor(x => x.HiringDate)
            .NotEmpty().WithMessage("Hiring date is required")
            .LessThanOrEqualTo(DateTime.Today).WithMessage("Hiring date cannot be in the future");

        RuleFor(x => x.ProbationPeriodMonths)
            .InclusiveBetween(0, 12).WithMessage("Probation period must be between 0 and 12 months");
    }

    private async Task<bool> EmployeeExists(Guid employeeId, CancellationToken cancellationToken)
    {
        return await _context.Employees.AnyAsync(e => e.Id == employeeId, cancellationToken);
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
