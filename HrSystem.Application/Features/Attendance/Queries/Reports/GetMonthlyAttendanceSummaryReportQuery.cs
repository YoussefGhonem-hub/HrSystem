using ErrorOr;
using HrSystem.Domain.Enums;
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

        // Include employees who are active but have zero attendance records in the month.
        var employeesWithRecords = records.Select(a => a.EmployeeId).ToHashSet();
        var allActiveEmployeesQuery = _context.Employees
            .AsNoTracking()
            .Include(e => e.Department)
            .Include(e => e.JobTitle)
            .Where(e => !e.IsDeleted && e.StatusId == EmployeeStatusIds.Active);
        if (branchId.HasValue)
            allActiveEmployeesQuery = allActiveEmployeesQuery.Where(e => e.BranchId == branchId.Value);
        if (request.DepartmentId.HasValue)
            allActiveEmployeesQuery = allActiveEmployeesQuery.Where(e => e.DepartmentId == request.DepartmentId.Value);
        if (request.EmployeeId.HasValue)
            allActiveEmployeesQuery = allActiveEmployeesQuery.Where(e => e.Id == request.EmployeeId.Value);
        var allActiveEmployees = await allActiveEmployeesQuery.ToListAsync(cancellationToken);

        // Load approved leave request dates per employee to count leave days that
        // may not have a corresponding OnLeave attendance record yet.
        var employeeIdsInRecords = records.Select(a => a.EmployeeId).Distinct().ToList();
        var approvedLeaveRequestDates = await _context.EmployeeRequests
            .AsNoTracking()
            .Include(r => r.RequestTypeRef)
            .Where(r =>
                employeeIdsInRecords.Contains(r.EmployeeId) &&
                (r.Status == Domain.Enums.EmployeeRequestStatus.Approved || r.Status == Domain.Enums.EmployeeRequestStatus.Completed) &&
                r.RequestTypeRef != null && r.RequestTypeRef.Code.ToLower() == "vacation" &&
                r.StartDate.HasValue && r.EndDate.HasValue &&
                r.StartDate.Value.Date <= toDate && r.EndDate.Value.Date >= fromDate)
            .Select(r => new { r.EmployeeId, Start = r.StartDate!.Value.Date, End = r.EndDate!.Value.Date })
            .ToListAsync(cancellationToken);

        // Build set of (employeeId, date) pairs covered by approved leave requests
        var leaveRequestDaySet = new HashSet<(Guid, DateTime)>();
        foreach (var lr in approvedLeaveRequestDates)
        {
            for (var d = lr.Start > fromDate ? lr.Start : fromDate; d <= lr.End && d <= toDate; d = d.AddDays(1))
            {
                if (IsWorkingDay(d, schedule) && !holidays.Contains(d.Date))
                    leaveRequestDaySet.Add((lr.EmployeeId, d));
            }
        }

        // Build rows from employees who have records in the month.
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
                var daysLate = g.Count(a => a.IsLate);
                var totalLateMin = g.Sum(a => a.LateMinutes.HasValue ? a.LateMinutes.Value.TotalMinutes : 0);
                var overtimeHours = g.Sum(a => a.OvertimeHours.HasValue ? a.OvertimeHours.Value.TotalHours : 0);

                // Leave days: explicit OnLeave records + approved leave request days without a record
                var daysWithOnLeaveRecord = g.Count(a => a.StatusId == AttendanceStatusIds.OnLeave);
                var recordDates = g.Select(a => a.Date.Date).ToHashSet();
                var leaveRequestDaysWithoutRecord = leaveRequestDaySet
                    .Count(kv => kv.Item1 == g.Key.EmployeeId && !recordDates.Contains(kv.Item2));
                var leaveDays = daysWithOnLeaveRecord + leaveRequestDaysWithoutRecord;

                // Absent days: explicit Absent records + working days with no record and not on leave
                var explicitAbsent = g.Count(a => a.StatusId == AttendanceStatusIds.Absent);
                var accountedDays = daysPresent + daysWithOnLeaveRecord + explicitAbsent
                    + g.Count(a => a.StatusId == AttendanceStatusIds.Holiday || a.StatusId == AttendanceStatusIds.Weekend);
                var daysAbsent = explicitAbsent + Math.Max(0, totalWorkingDays - accountedDays - leaveRequestDaysWithoutRecord);

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

        // Add fully-absent employees (no records at all in the month).
        var zeroRecordRows = allActiveEmployees
            .Where(e => !employeesWithRecords.Contains(e.Id))
            .Select(e => new EmployeeMonthlyAttendanceDto
            {
                EmployeeCode = e.EmployeeCode,
                EmployeeName = e.FirstNameEn + " " + e.LastNameEn,
                Department = e.Department?.NameEn ?? string.Empty,
                JobTitle = e.JobTitle?.TitleEn ?? string.Empty,
                DaysPresent = 0,
                DaysAbsent = totalWorkingDays,
                DaysLate = 0,
                TotalLateMinutes = 0,
                OvertimeHours = 0,
                LeaveDaysTaken = 0,
                AttendancePercentage = 0
            });

        var allRows = grouped.Concat(zeroRecordRows)
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
            Rows = allRows
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
