using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using AttendanceEntity = HrSystem.Domain.Entities.Attendance.Attendance;

namespace HrSystem.Application.Features.Attendance.Common;

/// <summary>
/// Single source of truth for all attendance-derived fields
/// (WorkedHours, IsLate/LateMinutes, IsEarlyLeave/EarlyLeaveMinutes,
/// IsOvertime/OvertimeHours, HalfDayRule and the derived StatusId).
///
/// Every code path that writes CheckInTime / CheckOutTime (check-in/out,
/// quick check, biometric verify, manual create/update, Excel import)
/// must call <see cref="ApplyAsync"/> so the persisted record is always
/// consistent with the employee's branch shift rules.
/// </summary>
public static class AttendanceMetricsCalculator
{
    /// <summary>
    /// Attendance statuses that are set manually by HR and are never derived
    /// from punch times. When a caller explicitly asks for one of these, the
    /// calculator keeps it instead of overriding it with the computed status.
    /// </summary>
    public static readonly IReadOnlySet<Guid> ManualStatuses = new HashSet<Guid>
    {
        AttendanceStatusIds.OnLeave,
        AttendanceStatusIds.Holiday,
        AttendanceStatusIds.Weekend
    };

    /// <summary>
    /// Recomputes WorkedHours from the punch times (null when either is missing
    /// or the check-out is before the check-in).
    /// </summary>
    public static void RecalculateWorkedHours(AttendanceEntity attendance)
    {
        if (attendance.CheckInTime.HasValue && attendance.CheckOutTime.HasValue
            && attendance.CheckOutTime.Value >= attendance.CheckInTime.Value)
        {
            attendance.WorkedHours = attendance.CheckOutTime.Value - attendance.CheckInTime.Value;
        }
        else
        {
            attendance.WorkedHours = null;
        }
    }

    /// <summary>
    /// Recomputes WorkedHours and every derived field from the branch work schedule.
    /// </summary>
    /// <param name="preserveStatusId">
    /// When supplied and it is one of <see cref="ManualStatuses"/>, the status is kept
    /// as-is (manual override); otherwise the status is derived from the punches.
    /// </param>
    public static async Task ApplyAsync(
        ApplicationDbContext context,
        AttendanceEntity attendance,
        Guid? branchId,
        CancellationToken cancellationToken,
        Guid? preserveStatusId = null)
    {
        RecalculateWorkedHours(attendance);

        attendance.IsLate = false;
        attendance.LateMinutes = null;
        attendance.IsEarlyLeave = false;
        attendance.EarlyLeaveMinutes = null;
        attendance.IsOvertime = false;
        attendance.OvertimeHours = null;

        var keepManualStatus = preserveStatusId.HasValue && ManualStatuses.Contains(preserveStatusId.Value);

        if (!branchId.HasValue || branchId.Value == Guid.Empty)
        {
            attendance.HalfDayRule = DescribeWithoutSchedule(attendance);
            attendance.StatusId = keepManualStatus ? preserveStatusId!.Value : AttendanceStatusIds.Present;
            return;
        }

        var schedule = await context.BranchWorkSchedules
            .AsNoTracking()
            .Where(s => s.BranchId == branchId.Value && s.IsActive && !s.IsDeleted)
            .OrderByDescending(s => s.IsDefault)
            .ThenBy(s => s.StartTime)
            .FirstOrDefaultAsync(cancellationToken);

        if (schedule == null)
        {
            attendance.HalfDayRule = DescribeWithoutSchedule(attendance);
            attendance.StatusId = keepManualStatus ? preserveStatusId!.Value : AttendanceStatusIds.Present;
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

        Guid derivedStatusId;

        if (checkInWindowClosed)
        {
            derivedStatusId = AttendanceStatusIds.Absent;
            attendance.HalfDayRule = "ABSENT";
        }
        else if (hasCompletePunch)
        {
            var workedHours = (decimal)effectiveWorkedHours.TotalHours;

            if (workedHours >= schedule.MinimumFullDayHours)
            {
                derivedStatusId = AttendanceStatusIds.Present;
                attendance.HalfDayRule = "FULL_DAY";
            }
            else if (workedHours >= schedule.MinimumHalfDayHours)
            {
                derivedStatusId = AttendanceStatusIds.Present;
                attendance.HalfDayRule = "HALF_DAY";
            }
            else
            {
                derivedStatusId = AttendanceStatusIds.Absent;
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
            derivedStatusId = hasAnyPunch ? AttendanceStatusIds.Present : AttendanceStatusIds.Absent;
            attendance.HalfDayRule = hasAnyPunch ? "INCOMPLETE" : "ABSENT";
        }

        attendance.StatusId = keepManualStatus ? preserveStatusId!.Value : derivedStatusId;
    }

    private static string DescribeWithoutSchedule(AttendanceEntity attendance)
    {
        if (attendance.CheckInTime.HasValue && attendance.CheckOutTime.HasValue)
            return "FULL_DAY";

        return attendance.CheckInTime.HasValue || attendance.CheckOutTime.HasValue
            ? "INCOMPLETE"
            : "ABSENT";
    }

    /// <summary>
    /// Backward compatible: values like 00:15 are treated as an offset from shift start.
    /// Values like 09:00 are treated as an absolute check-in cutoff time.
    /// </summary>
    public static TimeSpan ResolveLateCutoff(TimeSpan shiftStartTime, TimeSpan? gracePeriodLate)
    {
        if (!gracePeriodLate.HasValue)
            return shiftStartTime;

        return gracePeriodLate.Value >= TimeSpan.FromHours(2)
            ? gracePeriodLate.Value
            : shiftStartTime + gracePeriodLate.Value;
    }

    /// <summary>
    /// Backward compatible: values like 00:15 are treated as an offset before shift end.
    /// Values like 13:00 are treated as an absolute minimum checkout time.
    /// </summary>
    public static TimeSpan ResolveEarlyLeaveCutoff(TimeSpan shiftEndTime, TimeSpan? gracePeriodEarlyLeave)
    {
        if (!gracePeriodEarlyLeave.HasValue)
            return shiftEndTime;

        return gracePeriodEarlyLeave.Value >= TimeSpan.FromHours(2)
            ? gracePeriodEarlyLeave.Value
            : shiftEndTime - gracePeriodEarlyLeave.Value;
    }
}
