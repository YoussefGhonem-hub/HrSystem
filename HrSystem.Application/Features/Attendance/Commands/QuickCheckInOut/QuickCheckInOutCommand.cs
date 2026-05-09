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

        if (eventDate > DateTime.UtcNow.Date)
        {
            return Error.Validation(
                "Attendance.FutureDateNotAllowed",
                "Attendance date cannot be in the future.");
        }

        if (employee.HiringDate.HasValue && eventDate < employee.HiringDate.Value.Date)
        {
            var joiningDate = employee.HiringDate.Value.Date.ToString("yyyy-MM-dd");
            return Error.Validation(
                "Attendance.BeforeJoiningDate",
                $"Attendance date cannot be before employee joining date ({joiningDate}).");
        }

        var attendance = await _context.Attendances
            .Include(a => a.Employee)
            .Include(a => a.Status)
            .FirstOrDefaultAsync(a =>
                !a.IsDeleted &&
                !a.IsConfigurationRecord &&
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

        await ApplyAttendanceMetricsAsync(attendance, employee.BranchId, cancellationToken);
        if (attendance.StatusId == Guid.Empty)
        {
            attendance.StatusId = AttendanceStatusIds.Present;
        }
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
            HalfDayRule = attendance.HalfDayRule,
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

    private async Task ApplyAttendanceMetricsAsync(AttendanceEntity attendance, Guid? branchId, CancellationToken cancellationToken)
    {
        attendance.IsLate = false;
        attendance.LateMinutes = null;
        attendance.IsEarlyLeave = false;
        attendance.EarlyLeaveMinutes = null;
        attendance.IsOvertime = false;
        attendance.OvertimeHours = null;

        if (!branchId.HasValue || branchId.Value == Guid.Empty)
        {
            attendance.StatusId = AttendanceStatusIds.Present;
            return;
        }

        var schedule = await _context.BranchWorkSchedules
            .AsNoTracking()
            .Where(s => s.BranchId == branchId.Value && s.IsActive && !s.IsDeleted)
            .OrderByDescending(s => s.IsDefault)
            .ThenBy(s => s.StartTime)
            .FirstOrDefaultAsync(cancellationToken);

        if (schedule == null)
        {
            attendance.StatusId = AttendanceStatusIds.Present;
            return;
        }

        var hasCompletePunch = attendance.CheckInTime.HasValue && attendance.CheckOutTime.HasValue;
        var effectiveWorkedHours = attendance.WorkedHours ?? TimeSpan.Zero;

        if (hasCompletePunch && schedule.IsBreakTimeDeducted && schedule.BreakDuration.HasValue)
        {
            var deducted = effectiveWorkedHours - schedule.BreakDuration.Value;
            effectiveWorkedHours = deducted < TimeSpan.Zero ? TimeSpan.Zero : deducted;
            attendance.WorkedHours = effectiveWorkedHours;
        }

        var checkInWindowClosed = false;
        if (attendance.CheckInTime.HasValue && schedule.CheckInWindowMinutes.HasValue && schedule.CheckInWindowMinutes.Value > 0)
        {
            var checkInWindowEnd = schedule.StartTime.Add(TimeSpan.FromMinutes(schedule.CheckInWindowMinutes.Value));
            checkInWindowClosed = attendance.CheckInTime.Value > checkInWindowEnd;
        }

        if (attendance.CheckInTime.HasValue)
        {
            var allowedCheckIn = ResolveLateCutoff(schedule.StartTime, schedule.GracePeriodLate);
            if (attendance.CheckInTime.Value > allowedCheckIn)
            {
                attendance.IsLate = true;
                attendance.LateMinutes = attendance.CheckInTime.Value - allowedCheckIn;
            }
        }

        if (attendance.CheckOutTime.HasValue)
        {
            var allowedCheckOut = ResolveEarlyLeaveCutoff(schedule.EndTime, schedule.GracePeriodEarlyLeave);
            if (attendance.CheckOutTime.Value < allowedCheckOut)
            {
                attendance.IsEarlyLeave = true;
                attendance.EarlyLeaveMinutes = allowedCheckOut - attendance.CheckOutTime.Value;
            }
        }

        if (checkInWindowClosed)
        {
            attendance.StatusId = AttendanceStatusIds.Absent;
            attendance.HalfDayRule = "ABSENT";
            return;
        }

        if (hasCompletePunch)
        {
            var workedHours = (decimal)effectiveWorkedHours.TotalHours;
            var minimumFullDayHours = schedule.MinimumFullDayHours;
            var minimumHalfDayHours = schedule.MinimumHalfDayHours;
            var absentThresholdHours = schedule.AbsentThresholdHours;

            if (workedHours >= minimumFullDayHours)
            {
                attendance.StatusId = AttendanceStatusIds.Present;
                attendance.HalfDayRule = "FULL_DAY";
            }
            else if (workedHours >= minimumHalfDayHours)
            {
                attendance.StatusId = AttendanceStatusIds.Present;
                attendance.HalfDayRule = "HALF_DAY";
            }
            else if (workedHours < absentThresholdHours)
            {
                attendance.StatusId = AttendanceStatusIds.Absent;
                attendance.HalfDayRule = "ABSENT";
            }
            else
            {
                attendance.StatusId = AttendanceStatusIds.Absent;
                attendance.HalfDayRule = "ABSENT";
            }

            if (schedule.IsOvertimeEnabled && workedHours > schedule.OvertimeStartsAfterHours)
            {
                attendance.IsOvertime = true;
                attendance.OvertimeHours = TimeSpan.FromHours((double)(workedHours - schedule.OvertimeStartsAfterHours));
            }
        }
        else
        {
            var hasAnyPunch = attendance.CheckInTime.HasValue || attendance.CheckOutTime.HasValue;
            attendance.StatusId = hasAnyPunch ? AttendanceStatusIds.Present : AttendanceStatusIds.Absent;
            attendance.HalfDayRule = hasAnyPunch ? "INCOMPLETE" : "ABSENT";
        }
    }

    private static TimeSpan ResolveLateCutoff(TimeSpan shiftStartTime, TimeSpan? gracePeriodLate)
    {
        if (!gracePeriodLate.HasValue)
        {
            return shiftStartTime;
        }

        return gracePeriodLate.Value >= TimeSpan.FromHours(2)
            ? gracePeriodLate.Value
            : shiftStartTime + gracePeriodLate.Value;
    }

    private static TimeSpan ResolveEarlyLeaveCutoff(TimeSpan shiftEndTime, TimeSpan? gracePeriodEarlyLeave)
    {
        if (!gracePeriodEarlyLeave.HasValue)
        {
            return shiftEndTime;
        }

        return gracePeriodEarlyLeave.Value >= TimeSpan.FromHours(2)
            ? gracePeriodEarlyLeave.Value
            : shiftEndTime - gracePeriodEarlyLeave.Value;
    }
}
