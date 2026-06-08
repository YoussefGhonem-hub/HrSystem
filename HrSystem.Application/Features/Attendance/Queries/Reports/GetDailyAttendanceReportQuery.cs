using ErrorOr;
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

public class GetDailyAttendanceReportQueryHandler
    : IRequestHandler<GetDailyAttendanceReportQuery, ErrorOr<GenericResponse<DailyAttendanceReportDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetDailyAttendanceReportQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<DailyAttendanceReportDto>>> Handle(
        GetDailyAttendanceReportQuery request,
        CancellationToken cancellationToken)
    {
        var reportDate = request.Date?.Date ?? DateTime.UtcNow.Date;

        var isSuperOrOrgAdmin = CurrentUser.IsOrganizationAdmin || CurrentUser.IsSuperAdmin;
        var branchId = isSuperOrOrgAdmin ? (Guid?)null : CurrentUser.BranchId;

        // Start from all active employees so the report includes everyone regardless of
        // whether they have an attendance record for the date.
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

        // Load attendance records for those employees on the report date.
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

        var rows = employees.Select(e =>
        {
            attendanceByEmployee.TryGetValue(e.Id, out var att);
            return new DailyAttendanceRowDto
            {
                EmployeeCode = e.EmployeeCode,
                EmployeeName = e.FirstNameEn + " " + e.LastNameEn,
                Department = e.Department?.NameEn ?? string.Empty,
                JobTitle = e.JobTitle?.TitleEn ?? string.Empty,
                Status = att?.Status?.NameEn ?? "Absent",
                CheckIn = att?.CheckInTime,
                CheckOut = att?.CheckOutTime,
                LateMinutes = att?.LateMinutes.HasValue == true ? (int)att.LateMinutes!.Value.TotalMinutes : 0,
                EarlyLeaveMinutes = att?.EarlyLeaveMinutes.HasValue == true ? (int)att.EarlyLeaveMinutes!.Value.TotalMinutes : 0,
                WorkedHours = att?.WorkedHours.HasValue == true ? Math.Round(att.WorkedHours!.Value.TotalHours, 2) : 0
            };
        }).OrderBy(r => r.Department).ThenBy(r => r.EmployeeName).ToList();

        var presentCount = attendanceRecords.Count(a => a.StatusId == AttendanceStatusIds.Present || a.StatusId == AttendanceStatusIds.Late);
        var absentCount = employees.Count - attendanceByEmployee.Count
            + attendanceRecords.Count(a => a.StatusId == AttendanceStatusIds.Absent);
        var lateCount = attendanceRecords.Count(a => a.IsLate);
        var earlyLeaveCount = attendanceRecords.Count(a => a.IsEarlyLeave);
        var onLeaveCount = attendanceRecords.Count(a => a.StatusId == AttendanceStatusIds.OnLeave);
        var totalEmployees = employees.Count;
        var attendanceRate = totalEmployees > 0
            ? Math.Round((double)presentCount / totalEmployees * 100, 2)
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
            AttendanceRate = attendanceRate,
            Rows = rows
        };

        return GenericResponse<DailyAttendanceReportDto>.SuccessResult(dto, "Daily attendance report generated successfully.");
    }
}
