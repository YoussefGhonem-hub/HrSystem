using ErrorOr;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Attendance.Queries.Reports;

public record GetPayrollAttendanceReportQuery(
    int Month,
    int Year,
    Guid? DepartmentId = null,
    Guid? EmployeeId = null
) : IRequest<ErrorOr<GenericResponse<PayrollAttendanceReportDto>>>;

public class GetPayrollAttendanceReportQueryHandler
    : IRequestHandler<GetPayrollAttendanceReportQuery, ErrorOr<GenericResponse<PayrollAttendanceReportDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetPayrollAttendanceReportQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<PayrollAttendanceReportDto>>> Handle(
        GetPayrollAttendanceReportQuery request,
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

        // Get BranchWorkSchedule to calculate working days
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

        // Start from all active employees to include those with no check-in records.
        var empQuery = _context.Employees
            .AsNoTracking()
            .Include(e => e.Department)
            .Include(e => e.Salaries)
            .Where(e => !e.IsDeleted && e.StatusId == EmployeeStatusIds.Active);

        if (branchId.HasValue)
            empQuery = empQuery.Where(e => e.BranchId == branchId);

        if (request.DepartmentId.HasValue)
            empQuery = empQuery.Where(e => e.DepartmentId == request.DepartmentId);

        if (request.EmployeeId.HasValue)
            empQuery = empQuery.Where(e => e.Id == request.EmployeeId);

        var employees = await empQuery.ToListAsync(cancellationToken);
        var employeeIds = employees.Select(e => e.Id).ToList();

        // Attendance records for the month
        var records = await _context.Attendances
            .AsNoTracking()
            .Where(a => !a.IsDeleted
                && !a.IsConfigurationRecord
                && a.Date.Date >= fromDate
                && a.Date.Date <= toDate
                && employeeIds.Contains(a.EmployeeId))
            .ToListAsync(cancellationToken);

        var recordsByEmployee = records
            .GroupBy(a => a.EmployeeId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var rows = employees.Select(emp =>
            {
                recordsByEmployee.TryGetValue(emp.Id, out var empRecords);
                empRecords ??= new List<Domain.Entities.Attendance.Attendance>();

                var currentSalary = emp.Salaries
                    .Where(s => s.IsCurrent && !s.IsDeleted)
                    .OrderByDescending(s => s.EffectiveDate)
                    .FirstOrDefault();

                var basicSalary = currentSalary?.BasicSalary ?? 0m;
                var overtimeMultiplier = currentSalary?.OvertimeMultiplier ?? 1.5m;
                var workedDays = empRecords.Count(a => a.StatusId == AttendanceStatusIds.Present || a.StatusId == AttendanceStatusIds.Late);
                var absentDays = empRecords.Count(a => a.StatusId == AttendanceStatusIds.Absent);
                var totalLateMin = empRecords.Sum(a => a.LateMinutes.HasValue ? a.LateMinutes.Value.TotalMinutes : 0);
                var otHours = empRecords.Sum(a => a.OvertimeHours.HasValue ? a.OvertimeHours.Value.TotalHours : 0);

                var dailyRate = totalWorkingDays > 0 ? basicSalary / totalWorkingDays : 0m;
                var hourlyRate = dailyRate / 8m;

                var absentDeduction = absentDays * dailyRate;
                var lateDeduction = (decimal)(totalLateMin / 60.0) * hourlyRate;
                var otAmount = (decimal)otHours * hourlyRate * overtimeMultiplier;
                var netImpact = otAmount - absentDeduction - lateDeduction;

                return new PayrollAttendanceRowDto
                {
                    EmployeeCode = emp.EmployeeCode,
                    EmployeeName = emp.FirstNameEn + " " + emp.LastNameEn,
                    Department = emp.Department?.NameEn ?? string.Empty,
                    BasicSalary = basicSalary,
                    TotalWorkingDays = totalWorkingDays,
                    WorkedDays = workedDays,
                    AbsentDays = absentDays,
                    LateMinutes = Math.Round(totalLateMin, 1),
                    DailyRate = Math.Round(dailyRate, 2),
                    AbsentDeductionAmount = Math.Round(absentDeduction, 2),
                    LateDeductionAmount = Math.Round(lateDeduction, 2),
                    OvertimeHours = Math.Round(otHours, 2),
                    OvertimeRate = overtimeMultiplier,
                    OvertimeAmount = Math.Round(otAmount, 2),
                    NetSalaryImpact = Math.Round(netImpact, 2)
                };
            })
            .OrderBy(r => r.Department).ThenBy(r => r.EmployeeName)
            .ToList();

        var dto = new PayrollAttendanceReportDto
        {
            Meta = new AttendanceReportMeta
            {
                ReportType = "Payroll Attendance Report",
                GeneratedAt = DateTime.UtcNow,
                FromDate = fromDate,
                ToDate = toDate
            },
            Month = request.Month,
            Year = request.Year,
            TotalWorkingDays = totalWorkingDays,
            TotalLateDeductions = rows.Sum(r => r.LateDeductionAmount),
            TotalOvertimeAmounts = rows.Sum(r => r.OvertimeAmount),
            Rows = rows
        };

        return GenericResponse<PayrollAttendanceReportDto>.SuccessResult(dto, "Payroll attendance report generated successfully.");
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
            return day.DayOfWeek != DayOfWeek.Friday && day.DayOfWeek != DayOfWeek.Saturday;

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
