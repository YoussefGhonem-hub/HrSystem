using ErrorOr;
using HrSystem.Application.Features.Attendance.Common;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Attendance.Queries.Reports;

public record GetDailyAttendanceReportQuery(
    DateTime? Date = null,
    Guid? DepartmentId = null,
    Guid? EmployeeId = null
) : IRequest<ErrorOr<GenericResponse<DailyAttendanceReportDto>>>;

/// <summary>
/// Daily attendance report. Returns EVERY active employee in scope for the selected date,
/// regardless of whether an attendance record exists. Employees without a record are
/// classified as Holiday / Weekend / On Leave / Absent from the branch calendar and
/// approved vacation requests, so nobody silently disappears from the report.
/// </summary>
public class GetDailyAttendanceReportQueryHandler
    : IRequestHandler<GetDailyAttendanceReportQuery, ErrorOr<GenericResponse<DailyAttendanceReportDto>>>
{
    public const string StatusPresent = "Present";
    public const string StatusAbsent = "Absent";
    public const string StatusLate = "Late";
    public const string StatusEarlyLeave = "Early Leave";
    public const string StatusOnLeave = "On Leave";
    public const string StatusHoliday = "Holiday";
    public const string StatusWeekend = "Weekend";

    private readonly ApplicationDbContext _context;

    public GetDailyAttendanceReportQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<DailyAttendanceReportDto>>> Handle(
        GetDailyAttendanceReportQuery request,
        CancellationToken cancellationToken)
    {
        var reportDate = request.Date?.Date ?? DateTime.UtcNow.Date;

        var isSuperOrOrgAdmin = CurrentUser.IsOrganizationAdmin || CurrentUser.IsSuperAdmin;
        var branchId = isSuperOrOrgAdmin ? (Guid?)null : CurrentUser.BranchId;

        // 1) Start from all active employees so the report includes everyone.
        var employeeQuery = _context.Employees
            .AsNoTracking()
            .Include(e => e.Department)
            .Include(e => e.JobTitle)
            .Where(e => !e.IsDeleted && e.StatusId == EmployeeStatusIds.Active);

        if (branchId.HasValue)
            employeeQuery = employeeQuery.Where(e => e.BranchId == branchId);

        if (request.DepartmentId.HasValue)
            employeeQuery = employeeQuery.Where(e => e.DepartmentId == request.DepartmentId);

        if (request.EmployeeId.HasValue)
            employeeQuery = employeeQuery.Where(e => e.Id == request.EmployeeId);

        var employees = await employeeQuery.ToListAsync(cancellationToken);
        var employeeIds = employees.Select(e => e.Id).ToList();
        var branchIds = employees.Where(e => e.BranchId.HasValue).Select(e => e.BranchId!.Value).Distinct().ToList();

        // 2) Attendance records for those employees on the report date.
        var attendanceRecords = await _context.Attendances
            .AsNoTracking()
            .Include(a => a.Status)
            .Where(a => !a.IsDeleted
                && !a.IsConfigurationRecord
                && a.Date.Date == reportDate
                && employeeIds.Contains(a.EmployeeId))
            .ToListAsync(cancellationToken);

        var attendanceByEmployee = attendanceRecords
            .GroupBy(a => a.EmployeeId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(a => a.CreatedDate).First());

        // 3) Calendar context: weekends, holidays and approved vacations.
        var calendar = await AttendanceCalendar.LoadAsync(_context, branchIds, reportDate, reportDate, cancellationToken);
        var onLeaveEmployeeIds = await AttendanceCalendar.GetOnLeaveEmployeeIdsAsync(_context, employeeIds, reportDate, cancellationToken);

        var rows = employees.Select(e =>
        {
            attendanceByEmployee.TryGetValue(e.Id, out var att);
            var status = ResolveStatus(att, e.Id, e.BranchId, reportDate, calendar, onLeaveEmployeeIds);

            return new DailyAttendanceRowDto
            {
                EmployeeId = e.Id,
                EmployeeCode = e.EmployeeCode,
                EmployeeName = e.FirstNameEn + " " + e.LastNameEn,
                Department = e.Department?.NameEn ?? string.Empty,
                JobTitle = e.JobTitle?.TitleEn ?? string.Empty,
                Status = status,
                CheckIn = att?.CheckInTime,
                CheckOut = att?.CheckOutTime,
                IsLate = att?.IsLate == true,
                IsEarlyLeave = att?.IsEarlyLeave == true,
                LateMinutes = att?.LateMinutes.HasValue == true ? (int)att.LateMinutes!.Value.TotalMinutes : 0,
                EarlyLeaveMinutes = att?.EarlyLeaveMinutes.HasValue == true ? (int)att.EarlyLeaveMinutes!.Value.TotalMinutes : 0,
                WorkedHours = att?.WorkedHours.HasValue == true ? Math.Round(att.WorkedHours!.Value.TotalHours, 2) : 0,
                OvertimeHours = att?.OvertimeHours.HasValue == true ? Math.Round(att.OvertimeHours!.Value.TotalHours, 2) : 0,
                Notes = att?.Notes
            };
        }).OrderBy(r => r.Department).ThenBy(r => r.EmployeeName).ToList();

        var presentCount = rows.Count(r => r.Status is StatusPresent or StatusLate or StatusEarlyLeave);
        var absentCount = rows.Count(r => r.Status == StatusAbsent);
        var lateCount = rows.Count(r => r.IsLate);
        var earlyLeaveCount = rows.Count(r => r.IsEarlyLeave);
        var onLeaveCount = rows.Count(r => r.Status == StatusOnLeave);
        var holidayCount = rows.Count(r => r.Status == StatusHoliday);
        var weekendCount = rows.Count(r => r.Status == StatusWeekend);
        var totalEmployees = employees.Count;

        // Attendance rate is measured against employees expected to work that day.
        var expectedToWork = totalEmployees - onLeaveCount - holidayCount - weekendCount;
        var attendanceRate = expectedToWork > 0
            ? Math.Round((double)presentCount / expectedToWork * 100, 2)
            : 0;

        var dto = new DailyAttendanceReportDto
        {
            Meta = new AttendanceReportMeta
            {
                ReportType = "Daily Attendance Report",
                GeneratedAt = DateTime.UtcNow,
                FromDate = reportDate,
                ToDate = reportDate
            },
            ReportDate = reportDate,
            TotalEmployees = totalEmployees,
            PresentCount = presentCount,
            AbsentCount = absentCount,
            LateCount = lateCount,
            EarlyLeaveCount = earlyLeaveCount,
            OnLeaveCount = onLeaveCount,
            HolidayCount = holidayCount,
            WeekendCount = weekendCount,
            AttendanceRate = attendanceRate,
            Rows = rows
        };

        return GenericResponse<DailyAttendanceReportDto>.SuccessResult(dto, "Daily attendance report generated successfully.");
    }

    private static string ResolveStatus(
        Domain.Entities.Attendance.Attendance? att,
        Guid employeeId,
        Guid? employeeBranchId,
        DateTime reportDate,
        AttendanceCalendar calendar,
        HashSet<Guid> onLeaveEmployeeIds)
    {
        if (att != null)
        {
            if (att.StatusId == AttendanceStatusIds.OnLeave) return StatusOnLeave;
            if (att.StatusId == AttendanceStatusIds.Holiday) return StatusHoliday;
            if (att.StatusId == AttendanceStatusIds.Weekend) return StatusWeekend;
            if (att.StatusId == AttendanceStatusIds.Absent) return StatusAbsent;

            var hasAnyPunch = att.CheckInTime.HasValue || att.CheckOutTime.HasValue;
            if (att.StatusId == AttendanceStatusIds.Late || (att.IsLate && hasAnyPunch)) return StatusLate;
            if (att.StatusId == AttendanceStatusIds.EarlyLeave || (att.IsEarlyLeave && hasAnyPunch)) return StatusEarlyLeave;
            if (att.StatusId == AttendanceStatusIds.Present || hasAnyPunch) return StatusPresent;

            return att.Status?.NameEn ?? StatusAbsent;
        }

        if (onLeaveEmployeeIds.Contains(employeeId)) return StatusOnLeave;
        if (calendar.IsHoliday(reportDate, employeeBranchId)) return StatusHoliday;
        if (calendar.IsWeekend(reportDate, employeeBranchId)) return StatusWeekend;

        return StatusAbsent;
    }
}
