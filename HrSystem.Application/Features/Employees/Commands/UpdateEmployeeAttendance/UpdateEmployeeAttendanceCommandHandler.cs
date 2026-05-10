using System;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using HrSystem.Application.Features.Employees.Commands;
using HrSystem.Application.Features.Employees.Commands.CreateEmployee;
using HrSystem.Application.Features.Employees.Queries.GetEmployeeById;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Employees.Commands.UpdateEmployeeAttendance;

public class UpdateEmployeeAttendanceCommandHandler : IRequestHandler<UpdateEmployeeAttendanceCommand, ErrorOr<GenericResponse<EmployeeDto>>>
{
    private readonly ApplicationDbContext _context;

    public UpdateEmployeeAttendanceCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<EmployeeDto>>> Handle(UpdateEmployeeAttendanceCommand request, CancellationToken cancellationToken)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        if (employee is null)
        {
            return Error.NotFound(description: "Employee not found");
        }

        var editAccess = await EmployeeEditAuthorizationGuard.EnsureCanEditAsync(
            _context,
            employee.Id,
            employee.UserId,
            cancellationToken);
        if (editAccess.IsError)
        {
            return editAccess.Errors;
        }

        var configuration = await _context.Attendances
            .FirstOrDefaultAsync(a => a.EmployeeId == request.EmployeeId && a.IsConfigurationRecord, cancellationToken);

        if (configuration is null)
        {
            configuration = new HrSystem.Domain.Entities.Attendance.Attendance
            {
                EmployeeId = employee.Id,
                Date = (employee.HiringDate ?? DateTime.UtcNow).Date,
                StatusId = AttendanceStatusIds.Present,
                IsConfigurationRecord = true
            };

            await _context.Attendances.AddAsync(configuration, cancellationToken);
        }

        configuration.WorkShift = request.WorkShift;
        configuration.WorkDays = request.WorkDays;
        configuration.GracePeriod = request.GracePeriod;
        configuration.MaxLatePerMonth = request.MaxLatePerMonth;
        configuration.OvertimeEligible = request.OvertimeEligible;
        configuration.AttendanceMethod = request.AttendanceMethod;
        configuration.LateDeductionPolicy = request.LateDeductionPolicy;
        configuration.AbsenceDeductionPolicy = request.AbsenceDeductionPolicy;
        configuration.HalfDayRule = request.HalfDayRule;
        configuration.MissingCheckoutHandling = request.MissingCheckoutHandling;

        await _context.SaveChangesAsync(cancellationToken);

        var dto = await EmployeeCommandHelper.BuildEmployeeDtoAsync(_context, employee.Id, cancellationToken);

        return new GenericResponse<EmployeeDto>
        {
            Success = true,
            Message = "Employee attendance configuration updated successfully",
            Data = dto
        };
    }
}
