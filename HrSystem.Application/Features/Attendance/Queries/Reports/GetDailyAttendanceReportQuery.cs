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

        var query = _context.Attendances
            .AsNoTracking()
            .Include(a => a.Employee)
                .ThenInclude(e => e.Department)
            .Include(a => a.Employee)
                .ThenInclude(e => e.JobTitle)
            .Include(a => a.Status)
            .Where(a => !a.IsDeleted
                && !a.IsConfigurationRecord
                && a.Date.Date == reportDate);

        if (branchId.HasValue)
            query = query.Where(a => a.Employee.BranchId == branchId);

        if (request.DepartmentId.HasValue)
            query = query.Where(a => a.Employee.DepartmentId == request.DepartmentId);

        if (request.EmployeeId.HasValue)
            query = query.Where(a => a.EmployeeId == request.EmployeeId);

        var records = await query.ToListAsync(cancellationToken);

        var rows = records.Select(a => new DailyAttendanceRowDto
        {
            EmployeeCode = a.Employee.EmployeeCode,
            EmployeeName = a.Employee.FirstNameEn + " " + a.Employee.LastNameEn,
            Department = a.Employee.Department?.NameEn ?? string.Empty,
            JobTitle = a.Employee.JobTitle?.TitleEn ?? string.Empty,
            Status = a.Status?.NameEn ?? string.Empty,
            CheckIn = a.CheckInTime,
            CheckOut = a.CheckOutTime,
            LateMinutes = a.LateMinutes.HasValue ? (int)a.LateMinutes.Value.TotalMinutes : 0,
            EarlyLeaveMinutes = a.EarlyLeaveMinutes.HasValue ? (int)a.EarlyLeaveMinutes.Value.TotalMinutes : 0,
            WorkedHours = a.WorkedHours.HasValue ? Math.Round(a.WorkedHours.Value.TotalHours, 2) : 0
        }).OrderBy(r => r.Department).ThenBy(r => r.EmployeeName).ToList();

        var presentCount = records.Count(a => a.StatusId == AttendanceStatusIds.Present || a.StatusId == AttendanceStatusIds.Late);
        var absentCount = records.Count(a => a.StatusId == AttendanceStatusIds.Absent);
        var lateCount = records.Count(a => a.IsLate);
        var earlyLeaveCount = records.Count(a => a.IsEarlyLeave);
        var onLeaveCount = records.Count(a => a.StatusId == AttendanceStatusIds.OnLeave);
        var totalEmployees = records.Count;
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
