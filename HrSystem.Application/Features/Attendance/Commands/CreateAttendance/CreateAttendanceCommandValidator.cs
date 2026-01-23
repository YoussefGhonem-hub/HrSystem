using FluentValidation;
using HrSystem.Infrustructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Attendance.Commands.CreateAttendance;

public class CreateAttendanceCommandValidator : AbstractValidator<CreateAttendanceCommand>
{
    private readonly ApplicationDbContext _context;

    public CreateAttendanceCommandValidator(ApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("Employee ID is required")
            .MustAsync(EmployeeExists).WithMessage("Employee does not exist");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required")
            .LessThanOrEqualTo(DateTime.Today).WithMessage("Date cannot be in the future")
            .MustAsync(BeUniqueAttendance).WithMessage("Attendance record already exists for this employee on this date");

        RuleFor(x => x.StatusId)
            .NotEmpty().WithMessage("Status is required");

        RuleFor(x => x.CheckOutTime)
            .GreaterThan(x => x.CheckInTime)
            .WithMessage("Check-out time must be after check-in time")
            .When(x => x.CheckInTime.HasValue && x.CheckOutTime.HasValue);
    }

    private async Task<bool> EmployeeExists(Guid employeeId, CancellationToken cancellationToken)
    {
        return await _context.Employees.AnyAsync(e => e.Id == employeeId, cancellationToken);
    }

    private async Task<bool> BeUniqueAttendance(CreateAttendanceCommand command, DateTime date, CancellationToken cancellationToken)
    {
        return !await _context.Attendances.AnyAsync(
            a => a.EmployeeId == command.EmployeeId && a.Date.Date == date.Date,
            cancellationToken);
    }
}
