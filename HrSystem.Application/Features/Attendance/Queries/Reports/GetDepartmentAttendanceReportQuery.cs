using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Attendance.Queries.Reports;

public record GetDepartmentAttendanceReportQuery(
    DateTime FromDate,
    DateTime ToDate,
    Guid? DepartmentId = null
) : IRequest<ErrorOr<GenericResponse<DepartmentAttendanceReportDto>>>;

public class GetDepartmentAttendanceReportQueryHandler
    : IRequestHandler<GetDepartmentAttendanceReportQuery, ErrorOr<GenericResponse<DepartmentAttendanceReportDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetDepartmentAttendanceReportQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<DepartmentAttendanceReportDto>>> Handle(
        GetDepartmentAttendanceReportQuery request,
        CancellationToken cancellationToken)
    {
        var from = request.FromDate.Date;
        var to = request.ToDate.Date;

        if (from > to)
            return Error.Validation(description: "FromDate must be before or equal to ToDate.");

        var isSuperOrOrgAdmin = CurrentUser.IsOrganizationAdmin || CurrentUser.IsSuperAdmin;
        var branchId = isSuperOrOrgAdmin ? (Guid?)null : CurrentUser.BranchId;

        // Calculate working days for the range
        var schedule = branchId.HasValue
            ? await _context.BranchWorkSchedules.AsNoTracking()
                .FirstOrDefaultAsync(s => s.BranchId == branchId.Value && s.IsActive && !s.IsDeleted, cancellationToken)
            : null;

        int totalWorkingDays = CountWorkingDays(from, to, schedule);

        var query = _context.Attendances
            .AsNoTracking()
            .Include(a => a.Employee)
                .ThenInclude(e => e.Department)
            .Where(a => !a.IsDeleted
                && !a.IsConfigurationRecord
                && a.Date.Date >= from
                && a.Date.Date <= to);

        if (branchId.HasValue)
            query = query.Where(a => a.Employee.BranchId == branchId);

        if (request.DepartmentId.HasValue)
            query = query.Where(a => a.Employee.DepartmentId == request.DepartmentId);

        var records = await query.ToListAsync(cancellationToken);

        var rows = records
            .GroupBy(a => a.Employee.Department?.NameEn ?? "Unassigned")
            .Select(g =>
            {
                var distinctEmployees = g.Select(a => a.EmployeeId).Distinct().Count();
                var presentCount = g.Count(a =>
                    a.StatusId == AttendanceStatusIds.Present || a.StatusId == AttendanceStatusIds.Late);
                var absentCount = g.Count(a => a.StatusId == AttendanceStatusIds.Absent);
                var lateInstances = g.Count(a => a.IsLate);
                var otHours = g.Sum(a => a.OvertimeHours.HasValue ? a.OvertimeHours.Value.TotalHours : 0);

                var possiblePresent = distinctEmployees * totalWorkingDays;
                var avgAttPct = possiblePresent > 0
                    ? Math.Round((double)presentCount / possiblePresent * 100, 2)
                    : 0;

                return new DepartmentAttendanceRowDto
                {
                    DepartmentName = g.Key,
                    TotalEmployees = distinctEmployees,
                    AverageAttendancePercentage = avgAttPct,
                    TotalAbsentDays = absentCount,
                    TotalLateInstances = lateInstances,
                    TotalOvertimeHours = Math.Round(otHours, 2)
                };
            })
            .OrderByDescending(r => r.AverageAttendancePercentage)
            .ToList();

        var dto = new DepartmentAttendanceReportDto
        {
            Meta = new AttendanceReportMeta
            {
                ReportType = "Department Attendance Report",
                GeneratedAt = DateTime.UtcNow,
                FromDate = from,
                ToDate = to
            },
            TotalWorkingDays = totalWorkingDays,
            Rows = rows
        };

        return GenericResponse<DepartmentAttendanceReportDto>.SuccessResult(dto, "Department attendance report generated successfully.");
    }

    private static int CountWorkingDays(DateTime from, DateTime to,
        Domain.Entities.Organization.BranchWorkSchedule? schedule)
    {
        int count = 0;
        for (var day = from.Date; day <= to.Date; day = day.AddDays(1))
        {
            if (IsWorkingDay(day, schedule)) count++;
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
