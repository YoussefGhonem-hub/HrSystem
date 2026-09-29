using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Attendance.Common;

/// <summary>
/// Resolves, per branch, whether a given date is a weekend (from the active branch work
/// schedule) or a holiday (from branch holidays), and which employees are on approved
/// vacation. Used by reports so that employees without an attendance record are classified
/// as Weekend / Holiday / On Leave instead of being counted as Absent.
/// </summary>
public sealed class AttendanceCalendar
{
    private readonly IReadOnlyDictionary<Guid, BranchScheduleDays> _scheduleByBranch;
    private readonly IReadOnlyDictionary<Guid, List<BranchHolidayInfo>> _holidayByBranch;

    private AttendanceCalendar(
        IReadOnlyDictionary<Guid, BranchScheduleDays> scheduleByBranch,
        IReadOnlyDictionary<Guid, List<BranchHolidayInfo>> holidayByBranch)
    {
        _scheduleByBranch = scheduleByBranch;
        _holidayByBranch = holidayByBranch;
    }

    public static async Task<AttendanceCalendar> LoadAsync(
        ApplicationDbContext context,
        IReadOnlyCollection<Guid> branchIds,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken)
    {
        var minDate = fromDate.Date;
        var maxDate = toDate.Date;

        var schedules = await context.BranchWorkSchedules
            .AsNoTracking()
            .Where(s => branchIds.Contains(s.BranchId) && s.IsActive && !s.IsDeleted)
            .Select(s => new
            {
                s.BranchId,
                s.IsDefault,
                s.CreatedDate,
                Days = new BranchScheduleDays(s.IsSunday, s.IsMonday, s.IsTuesday, s.IsWednesday, s.IsThursday, s.IsFriday, s.IsSaturday)
            })
            .ToListAsync(cancellationToken);

        var scheduleByBranch = schedules
            .GroupBy(s => s.BranchId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(s => s.IsDefault).ThenByDescending(s => s.CreatedDate).First().Days);

        var holidays = await context.BranchHolidays
            .AsNoTracking()
            .Where(h => branchIds.Contains(h.BranchId) &&
                        h.IsActive &&
                        !h.IsDeleted &&
                        ((!h.IsRecurring && h.Date.Date >= minDate && h.Date.Date <= maxDate) || h.IsRecurring))
            .Select(h => new BranchHolidayInfo(h.BranchId, h.Date.Date, h.IsRecurring, h.RecurringMonth, h.RecurringDay))
            .ToListAsync(cancellationToken);

        var holidayByBranch = holidays
            .GroupBy(h => h.BranchId)
            .ToDictionary(g => g.Key, g => g.ToList());

        return new AttendanceCalendar(scheduleByBranch, holidayByBranch);
    }

    /// <summary>
    /// Employee IDs that have an approved vacation request covering <paramref name="date"/>.
    /// </summary>
    public static async Task<HashSet<Guid>> GetOnLeaveEmployeeIdsAsync(
        ApplicationDbContext context,
        IReadOnlyCollection<Guid> employeeIds,
        DateTime date,
        CancellationToken cancellationToken)
    {
        var day = date.Date;

        var vacationRequestTypeId = await context.RequestTypes
            .AsNoTracking()
            .Where(rt => rt.Code == "Vacation")
            .Select(rt => rt.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (vacationRequestTypeId == Guid.Empty || employeeIds.Count == 0)
            return new HashSet<Guid>();

        var ids = await context.EmployeeRequests
            .AsNoTracking()
            .Where(r => !r.IsDeleted &&
                        r.RequestTypeId == vacationRequestTypeId &&
                        r.Status == EmployeeRequestStatus.Approved &&
                        employeeIds.Contains(r.EmployeeId) &&
                        r.StartDate.HasValue &&
                        r.EndDate.HasValue &&
                        r.StartDate.Value.Date <= day &&
                        r.EndDate.Value.Date >= day)
            .Select(r => r.EmployeeId)
            .Distinct()
            .ToListAsync(cancellationToken);

        return ids.ToHashSet();
    }

    public bool IsHoliday(DateTime date, Guid? branchId)
    {
        if (!branchId.HasValue || !_holidayByBranch.TryGetValue(branchId.Value, out var holidays))
            return false;

        var day = date.Date;
        foreach (var holiday in holidays)
        {
            if (!holiday.IsRecurring && holiday.Date == day)
                return true;

            if (holiday.IsRecurring &&
                holiday.RecurringMonth is int month &&
                holiday.RecurringDay is int dayOfMonth &&
                month == day.Month &&
                dayOfMonth == day.Day)
                return true;
        }

        return false;
    }

    public bool IsWeekend(DateTime date, Guid? branchId)
    {
        if (!branchId.HasValue || !_scheduleByBranch.TryGetValue(branchId.Value, out var schedule))
        {
            // No schedule configured: default to a Friday/Saturday weekend.
            return date.DayOfWeek is DayOfWeek.Friday or DayOfWeek.Saturday;
        }

        return date.DayOfWeek switch
        {
            DayOfWeek.Sunday => !schedule.IsSunday,
            DayOfWeek.Monday => !schedule.IsMonday,
            DayOfWeek.Tuesday => !schedule.IsTuesday,
            DayOfWeek.Wednesday => !schedule.IsWednesday,
            DayOfWeek.Thursday => !schedule.IsThursday,
            DayOfWeek.Friday => !schedule.IsFriday,
            DayOfWeek.Saturday => !schedule.IsSaturday,
            _ => false
        };
    }

    public sealed record BranchScheduleDays(
        bool IsSunday,
        bool IsMonday,
        bool IsTuesday,
        bool IsWednesday,
        bool IsThursday,
        bool IsFriday,
        bool IsSaturday);

    private sealed record BranchHolidayInfo(
        Guid BranchId,
        DateTime Date,
        bool IsRecurring,
        int? RecurringMonth,
        int? RecurringDay);
}
