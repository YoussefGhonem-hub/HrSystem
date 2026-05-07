using System.Security.Cryptography;
using System.Text;
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

namespace HrSystem.Application.Features.Attendance.Commands.VerifyBiometricAttendance;

public class VerifyBiometricAttendanceCommandHandler : IRequestHandler<VerifyBiometricAttendanceCommand, ErrorOr<GenericResponse<AttendanceDto>>>
{
    private readonly ApplicationDbContext _context;

    public VerifyBiometricAttendanceCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<AttendanceDto>>> Handle(
        VerifyBiometricAttendanceCommand request,
        CancellationToken cancellationToken)
    {
        Guid employeeId;
        if (request.EmployeeId.HasValue && request.EmployeeId.Value != Guid.Empty)
        {
            employeeId = request.EmployeeId.Value;
        }
        else
        {
            var currentEmployeeId = CurrentUser.EmployeeId;
            if (!currentEmployeeId.HasValue || currentEmployeeId.Value == Guid.Empty)
            {
                return Error.Unauthorized("Attendance.Unauthorized", "Current user is not linked to an employee");
            }

            employeeId = currentEmployeeId.Value;
        }

        var isHr = CurrentUser.Roles?.Contains(RoleNames.HRManager) == true ||
                   CurrentUser.Roles?.Contains(RoleNames.HRSpecialist) == true ||
                   CurrentUser.Roles?.Contains(RoleNames.OrganizationAdmin) == true ||
                   CurrentUser.Roles?.Contains(RoleNames.SuperAdmin) == true;

        if (!isHr && CurrentUser.EmployeeId != employeeId)
        {
            return Error.Unauthorized("Attendance.Unauthorized", "Not allowed to submit attendance for this employee");
        }

        var templateHash = ComputeTemplateHash(request.TemplateBase64);

        var biometric = await _context.EmployeeBiometrics
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.EmployeeId == employeeId && b.BiometricType == request.BiometricType && b.IsActive, cancellationToken);

        if (biometric == null)
        {
            return Error.NotFound("Biometric.NotFound", "No biometric enrolled for this employee");
        }

        if (!string.Equals(biometric.TemplateHash, templateHash, StringComparison.OrdinalIgnoreCase))
        {
            return Error.Unauthorized("Biometric.NotMatched", "Biometric verification failed");
        }

        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == employeeId, cancellationToken);

        if (employee == null)
        {
            return Error.NotFound("Employee.NotFound", "Employee not found");
        }

        var eventTime = request.EventTime ?? DateTime.UtcNow;
        var date = eventTime.Date;
        var time = eventTime.TimeOfDay;

        var attendance = await _context.Attendances
            .Include(a => a.Employee)
            .Include(a => a.Status)
            .FirstOrDefaultAsync(a => !a.IsDeleted && !a.IsConfigurationRecord && a.EmployeeId == employeeId && a.Date == date, cancellationToken);

        if (attendance == null)
        {
            attendance = new AttendanceEntity
            {
                EmployeeId = employeeId,
                Date = date,
                StatusId = AttendanceStatusIds.Present,
                DeviceId = request.DeviceId,
                TenantId = employee.TenantId
            };

            attendance.MarkAsCreated(CurrentUser.Id ?? Guid.Empty);
            _context.Attendances.Add(attendance);
        }

        if (request.PunchType == AttendancePunchType.CheckIn)
        {
            if (attendance.CheckInTime.HasValue)
            {
                return Error.Conflict("Attendance.AlreadyCheckedIn", "Employee already checked in");
            }

            attendance.CheckInTime = time;
            attendance.CheckInDeviceId = request.DeviceId;
            attendance.DeviceId ??= request.DeviceId;
        }
        else
        {
            if (attendance.CheckOutTime.HasValue)
            {
                return Error.Conflict("Attendance.AlreadyCheckedOut", "Employee already checked out");
            }

            attendance.CheckOutTime = time;
            attendance.CheckOutDeviceId = request.DeviceId;
            attendance.DeviceId ??= request.DeviceId;
        }

        if (attendance.CheckInTime.HasValue && attendance.CheckOutTime.HasValue)
        {
            attendance.WorkedHours = attendance.CheckOutTime.Value - attendance.CheckInTime.Value;
        }

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

        return new GenericResponse<AttendanceDto>
        {
            Success = true,
            Message = "Attendance recorded successfully",
            Data = dto
        };
    }

    private static string ComputeTemplateHash(string templateBase64)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(templateBase64.Trim());
        var hash = sha.ComputeHash(bytes);
        return Convert.ToHexString(hash);
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

        // Backward compatible: values like 00:15 are treated as offset from shift start.
        // Values like 09:00 are treated as an absolute check-in cutoff time.
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

        // Backward compatible: values like 00:15 are treated as offset before shift end.
        // Values like 13:00 are treated as an absolute minimum checkout time.
        return gracePeriodEarlyLeave.Value >= TimeSpan.FromHours(2)
            ? gracePeriodEarlyLeave.Value
            : shiftEndTime - gracePeriodEarlyLeave.Value;
    }
}
