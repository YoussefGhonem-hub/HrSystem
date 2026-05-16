using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Attendance.Queries.Reports;

public record GetMonthlyAttendanceSummaryReportQuery(
    int Month,
    int Year,
    Guid? DepartmentId = null,
    Guid? EmployeeId = null
) : IRequest<ErrorOr<GenericResponse<MonthlyAttendanceSummaryReportDto>>>;

public class GetMonthlyAttendanceSummaryReportQueryHandler
    : IRequestHandler<GetMonthlyAttendanceSummaryReportQuery, ErrorOr<GenericResponse<MonthlyAttendanceSummaryReportDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetMonthlyAttendanceSummaryReportQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<MonthlyAttendanceSummaryReportDto>>> Handle(
        GetMonthlyAttendanceSummaryReportQuery request,
        CancellationToken cancellationToken)
    {
        if (request.Month < 1 || request.Month > 12)
            return Error.Validation(description: "Month must be between 1 and 12.");
        if (request.Year < 2000 || request.Year > 2100)
            return Error.Validation(description: "Invalid year.");

        var fromDate = new DateTime(request.Year, request.Month, 1);
        var toDate = fromDate.AddMonths(1).AddDays(-1);

        var isSuperOrOrgAdmin = CurrentUser.IsOrganizationAdmin || CurrentUser.IsSuperAdmin;
        var branchId = isSuperOrOrgAdmin ? (Guid?)null : CurrentUser.BranchId;

        // Calculate total working days for the branch in this month
        var schedule = branchId.HasValue
            ? await _context.BranchWorkSchedules.AsNoTracking()
                .FirstOrDefaultAsync(s => s.BranchId == branchId.Value && s.IsActive && !s.IsDeleted, cancellationToken)
            : null;

        var holidays = await _context.BranchHolidays.AsNoTracking()
            .Where(h => !h.IsDeleted && h.IsActive
                && ((h.Year == request.Year && h.Date.Month == request.Month)
                    || (h.IsRecurring && h.RecurringMonth == request.Month)))
            .Select(h => h.Date.Date)
            .ToListAsync(cancellationToken);

        int totalWorkingDays = CountWorkingDays(fromDate, toDate, schedule, holidays);

        var query = _context.Attendances
            .AsNoTracking()
            .Include(a => a.Employee)
                .ThenInclude(e => e.Department)
            .Include(a => a.Employee)
                .ThenInclude(e => e.JobTitle)
            .Where(a => !a.IsDeleted
                && !a.IsConfigurationRecord
                && a.Date.Date >= fromDate
                && a.Date.Date <= toDate);

        if (branchId.HasValue)
            query = query.Where(a => a.Employee.BranchId == branchId);

        if (request.DepartmentId.HasValue)
            query = query.Where(a => a.Employee.DepartmentId == request.DepartmentId);

        if (request.EmployeeId.HasValue)
            query = query.Where(a => a.EmployeeId == request.EmployeeId);

        var records = await query.ToListAsync(cancellationToken);

        var grouped = records
            .GroupBy(a => new
            {
                a.EmployeeId,
                Code = a.Employee.EmployeeCode,
                Name = a.Employee.FirstNameEn + " " + a.Employee.LastNameEn,
                Dept = a.Employee.Department?.NameEn ?? string.Empty,
                Job = a.Employee.JobTitle?.TitleEn ?? string.Empty
            })
            .Select(g =>
            {
                var daysPresent = g.Count(a => a.StatusId == AttendanceStatusIds.Present || a.StatusId == AttendanceStatusIds.Late);
                var daysAbsent = g.Count(a => a.StatusId == AttendanceStatusIds.Absent);
                var daysLate = g.Count(a => a.IsLate);
                var totalLateMin = g.Sum(a => a.LateMinutes.HasValue ? a.LateMinutes.Value.TotalMinutes : 0);
                var overtimeHours = g.Sum(a => a.OvertimeHours.HasValue ? a.OvertimeHours.Value.TotalHours : 0);
                var leaveDays = g.Count(a => a.StatusId == AttendanceStatusIds.OnLeave);
                var attendancePct = totalWorkingDays > 0
                    ? Math.Round((double)daysPresent / totalWorkingDays * 100, 2)
                    : 0;

                return new EmployeeMonthlyAttendanceDto
                {
                    EmployeeCode = g.Key.Code,
                    EmployeeName = g.Key.Name,
                    Department = g.Key.Dept,
                    JobTitle = g.Key.Job,
                    DaysPresent = daysPresent,
                    DaysAbsent = daysAbsent,
                    DaysLate = daysLate,
                    TotalLateMinutes = Math.Round(totalLateMin, 1),
                    OvertimeHours = Math.Round(overtimeHours, 2),
                    LeaveDaysTaken = leaveDays,
                    AttendancePercentage = attendancePct
                };
            })
            .OrderBy(r => r.Department).ThenBy(r => r.EmployeeName)
            .ToList();

        var dto = new MonthlyAttendanceSummaryReportDto
        {
            Meta = new AttendanceReportMeta
            {
                ReportType = "Monthly Attendance Summary Report",
                GeneratedAt = DateTime.UtcNow,
                FromDate = fromDate,
                ToDate = toDate
            },
            Month = request.Month,
            Year = request.Year,
            TotalWorkingDays = totalWorkingDays,
            Rows = grouped
        };

        return GenericResponse<MonthlyAttendanceSummaryReportDto>.SuccessResult(dto, "Monthly attendance summary report generated successfully.");
    }

    private static int CountWorkingDays(
        DateTime from, DateTime to,
        Domain.Entities.Organization.BranchWorkSchedule? schedule,
        List<DateTime> holidays)
    {
        int count = 0;
        for (var day = from.Date; day <= to.Date; day = day.AddDays(1))
        {
            if (IsWorkingDay(day, schedule) && !holidays.Contains(day.Date))
                count++;
        }
        return count;
    }

    private static bool IsWorkingDay(DateTime day, Domain.Entities.Organization.BranchWorkSchedule? schedule)
    {
        if (schedule == null)
        {
            // Default: Sunday–Thursday working, Friday–Saturday off
            return day.DayOfWeek != DayOfWeek.Friday && day.DayOfWeek != DayOfWeek.Saturday;
        }

        return day.DayOfWeek switch
        {
            DayOfWeek.Sunday => schedule.IsSunday,
            DayOfWeek.Monday => schedule.IsMonday,
            DayOfWeek.Tuesday => schedule.IsTuesday,
            DayOfWeek.Wednesday => schedule.IsWednesday,
            DayOfWeek.Thursday => schedule.IsThursday,
            DayOfWeek.Friday => schedule.IsFriday,
            DayOfWeek.Saturday => schedule.IsSaturday,
            _ => false
        };
    }
}
