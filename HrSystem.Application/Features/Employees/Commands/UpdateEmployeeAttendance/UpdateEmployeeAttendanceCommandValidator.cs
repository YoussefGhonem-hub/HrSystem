using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using HrSystem.Infrustructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Employees.Commands.UpdateEmployeeAttendance;

public class UpdateEmployeeAttendanceCommandValidator : AbstractValidator<UpdateEmployeeAttendanceCommand>
{
    private readonly ApplicationDbContext _context;

    public UpdateEmployeeAttendanceCommandValidator(ApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("Employee ID is required")
            .MustAsync(EmployeeExists).WithMessage("Employee not found");

        RuleFor(x => x.WorkShift).MaximumLength(128);
        RuleFor(x => x.WorkDays).MaximumLength(128);
        RuleFor(x => x.GracePeriod).MaximumLength(64);
        RuleFor(x => x.MaxLatePerMonth).MaximumLength(64);
        RuleFor(x => x.AttendanceMethod).MaximumLength(128);
        RuleFor(x => x.LateDeductionPolicy).MaximumLength(128);
        RuleFor(x => x.AbsenceDeductionPolicy).MaximumLength(128);
        RuleFor(x => x.HalfDayRule).MaximumLength(128);
        RuleFor(x => x.MissingCheckoutHandling).MaximumLength(128);
    }

    private async Task<bool> EmployeeExists(Guid employeeId, CancellationToken cancellationToken)
    {
        return await _context.Employees.AnyAsync(e => e.Id == employeeId, cancellationToken);
    }
}
