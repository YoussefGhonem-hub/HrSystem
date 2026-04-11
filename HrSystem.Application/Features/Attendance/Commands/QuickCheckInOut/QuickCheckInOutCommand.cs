using ErrorOr;
using HrSystem.Application.Features.Attendance.Queries.GetAttendanceById;
using AttendanceEntity = HrSystem.Domain.Entities.Attendance.Attendance;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Attendance.Commands.QuickCheckInOut;

public record QuickCheckInOutCommand(
    Guid EmployeeId,
    AttendancePunchType PunchType,
    DateTime EventDateTime
) : IRequest<ErrorOr<GenericResponse<AttendanceDto>>>;

public class QuickCheckInOutCommandHandler
    : IRequestHandler<QuickCheckInOutCommand, ErrorOr<GenericResponse<AttendanceDto>>>
{
    private readonly ApplicationDbContext _context;

    public QuickCheckInOutCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<AttendanceDto>>> Handle(
        QuickCheckInOutCommand request,
        CancellationToken cancellationToken)
    {
        var isHr = CurrentUser.Roles?.Contains(RoleNames.HRManager) == true ||
                   CurrentUser.Roles?.Contains(RoleNames.HRSpecialist) == true ||
                   CurrentUser.Roles?.Contains(RoleNames.OrganizationAdmin) == true;

        if (!isHr)
            return Error.Forbidden("Attendance.Forbidden", "Only HR Manager and HR Specialist can use quick check-in/out.");

        var employee = await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId && !e.IsDeleted, cancellationToken);

        if (employee is null)
            return Error.NotFound("Employee.NotFound", "Employee not found.");

        if (!employee.BranchId.HasValue || employee.BranchId.Value == Guid.Empty)
            return Error.Validation("Employee.NoBranch", "Employee is not assigned to a branch.");

        var setting = await _context.BranchAttendanceSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.BranchId == employee.BranchId.Value && !s.IsDeleted, cancellationToken);

        var eventDate = request.EventDateTime.Date;
        var eventTime = request.EventDateTime.TimeOfDay;

        var attendance = await _context.Attendances
            .Include(a => a.Employee)
            .Include(a => a.Status)
            .FirstOrDefaultAsync(a =>
                !a.IsDeleted &&
                a.EmployeeId == request.EmployeeId &&
                a.Date == eventDate,
                cancellationToken);

        if (attendance is null)
        {
            attendance = new AttendanceEntity
            {
                EmployeeId = request.EmployeeId,
                Date = eventDate,
                StatusId = AttendanceStatusIds.Present,
                BranchId = employee.BranchId,
                TenantId = employee.TenantId
            };

            attendance.MarkAsCreated(CurrentUser.Id ?? Guid.Empty);
            _context.Attendances.Add(attendance);
        }

        if (request.PunchType == AttendancePunchType.CheckIn)
        {
            if (attendance.CheckInTime.HasValue && setting?.AllowMultipleCheckInsPerDay != true)
                return Error.Conflict("Attendance.AlreadyCheckedIn", "Employee already checked in today.");

            attendance.CheckInTime = eventTime;
            attendance.CheckInMethod = AttendanceMethod.Manual;
        }
        else
        {
            if (attendance.CheckOutTime.HasValue && setting?.AllowMultipleCheckInsPerDay != true)
                return Error.Conflict("Attendance.AlreadyCheckedOut", "Employee already checked out today.");

            if (attendance.CheckInTime.HasValue && setting?.MinCheckInDurationMinutes > 0)
            {
                var duration = eventTime - attendance.CheckInTime.Value;
                if (duration.TotalMinutes < setting.MinCheckInDurationMinutes)
                {
                    return Error.Validation("Attendance.TooShort",
                        $"Minimum time between check-in and check-out is {setting.MinCheckInDurationMinutes} minutes.");
                }
            }

            attendance.CheckOutTime = eventTime;
            attendance.CheckOutMethod = AttendanceMethod.Manual;
        }

        if (attendance.CheckInTime.HasValue && attendance.CheckOutTime.HasValue)
            attendance.WorkedHours = attendance.CheckOutTime.Value - attendance.CheckInTime.Value;

        attendance.StatusId = AttendanceStatusIds.Present;
        attendance.MarkAsModified(CurrentUser.Id ?? Guid.Empty);

        await _context.SaveChangesAsync(cancellationToken);

        attendance = await _context.Attendances
            .Include(a => a.Employee)
            .Include(a => a.Status)
            .FirstAsync(a => a.Id == attendance.Id, cancellationToken);

        var dto = new AttendanceDto
        {
            Id = attendance.Id,
            EmployeeId = attendance.EmployeeId,
            EmployeeName = attendance.Employee?.FullNameEn,
            EmployeeCode = attendance.Employee?.EmployeeCode,
            Date = attendance.Date,
            CheckInTime = attendance.CheckInTime,
            CheckOutTime = attendance.CheckOutTime,
            StatusId = attendance.StatusId,
            StatusNameEn = attendance.Status?.NameEn,
            StatusNameAr = attendance.Status?.NameAr,
            DeviceId = attendance.DeviceId,
            CheckInDeviceId = attendance.CheckInDeviceId,
            CheckOutDeviceId = attendance.CheckOutDeviceId,
            WorkedHours = attendance.WorkedHours,
            OvertimeHours = attendance.OvertimeHours,
            LateMinutes = attendance.LateMinutes,
            EarlyLeaveMinutes = attendance.EarlyLeaveMinutes,
            IsLate = attendance.IsLate,
            IsEarlyLeave = attendance.IsEarlyLeave,
            IsOvertime = attendance.IsOvertime,
            Notes = attendance.Notes,
            ApprovedBy = attendance.ApprovedBy,
            ApprovedDate = attendance.ApprovedDate
        };

        var action = request.PunchType == AttendancePunchType.CheckIn ? "Quick check-in" : "Quick check-out";

        return new GenericResponse<AttendanceDto>
        {
            Success = true,
            Message = $"{action} recorded successfully.",
            Data = dto
        };
    }
}
