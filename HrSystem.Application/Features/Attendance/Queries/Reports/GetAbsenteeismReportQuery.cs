using ErrorOr;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Attendance.Queries.Reports;

public record GetAbsenteeismReportQuery(
    DateTime FromDate,
    DateTime ToDate,
    Guid? DepartmentId = null,
    Guid? EmployeeId = null
) : IRequest<ErrorOr<GenericResponse<AbsenteeismReportDto>>>;

public class GetAbsenteeismReportQueryHandler
    : IRequestHandler<GetAbsenteeismReportQuery, ErrorOr<GenericResponse<AbsenteeismReportDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetAbsenteeismReportQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<AbsenteeismReportDto>>> Handle(
        GetAbsenteeismReportQuery request,
        CancellationToken cancellationToken)
    {
        var from = request.FromDate.Date;
        var to = request.ToDate.Date;

        if (from > to)
            return Error.Validation(description: "FromDate must be before or equal to ToDate.");

        var isSuperOrOrgAdmin = CurrentUser.IsOrganizationAdmin || CurrentUser.IsSuperAdmin;
        var branchId = isSuperOrOrgAdmin ? (Guid?)null : CurrentUser.BranchId;

        // Load absent attendance records
        var absenceQuery = _context.Attendances
            .AsNoTracking()
            .Include(a => a.Employee)
                .ThenInclude(e => e.Department)
            .Where(a => !a.IsDeleted
                && !a.IsConfigurationRecord
                && a.StatusId == AttendanceStatusIds.Absent
                && a.Date.Date >= from
                && a.Date.Date <= to);

        if (branchId.HasValue)
            absenceQuery = absenceQuery.Where(a => a.Employee.BranchId == branchId);

        if (request.DepartmentId.HasValue)
            absenceQuery = absenceQuery.Where(a => a.Employee.DepartmentId == request.DepartmentId);

        if (request.EmployeeId.HasValue)
            absenceQuery = absenceQuery.Where(a => a.EmployeeId == request.EmployeeId);

        var absences = await absenceQuery.ToListAsync(cancellationToken);

        // Load approved leave requests in the range to detect authorized absences
        var approvedLeaveReqs = await _context.EmployeeRequests
            .AsNoTracking()
            .Where(r => !r.IsDeleted
                && r.Status == EmployeeRequestStatus.Approved
                && r.StartDate.HasValue && r.StartDate.Value.Date >= from
                && r.EndDate.HasValue && r.EndDate.Value.Date <= to)
            .Select(r => new { r.EmployeeId, r.StartDate, r.EndDate, r.Title })
            .ToListAsync(cancellationToken);

        // Calculate total possible attendance days in range per employee
        var totalDaysInRange = (to - from).Days + 1;

        var rows = absences
            .GroupBy(a => new
            {
                a.EmployeeId,
                Code = a.Employee.EmployeeCode,
                Name = a.Employee.FirstNameEn + " " + a.Employee.LastNameEn,
                Dept = a.Employee.Department?.NameEn ?? string.Empty
            })
            .Select(g =>
            {
                var absentDays = g.Count();
                var authorized = 0;
                var reasons = new List<string>();

                foreach (var absence in g)
                {
                    var coveringLeave = approvedLeaveReqs.FirstOrDefault(r =>
                        r.EmployeeId == absence.EmployeeId
                        && absence.Date.Date >= r.StartDate!.Value.Date
                        && absence.Date.Date <= r.EndDate!.Value.Date);

                    if (coveringLeave != null)
                    {
                        authorized++;
                        var reason = coveringLeave.Title ?? "Approved Leave";
                        if (!reasons.Contains(reason))
                            reasons.Add(reason);
                    }
                    else if (!string.IsNullOrWhiteSpace(absence.Notes))
                    {
                        var note = absence.Notes!.Trim();
                        if (!reasons.Contains(note))
                            reasons.Add(note);
                    }
                }

                var absenceRate = totalDaysInRange > 0
                    ? Math.Round((double)absentDays / totalDaysInRange * 100, 2)
                    : 0;

                return new AbsenteeismRowDto
                {
                    EmployeeCode = g.Key.Code,
                    EmployeeName = g.Key.Name,
                    Department = g.Key.Dept,
                    AbsentDays = absentDays,
                    AuthorizedAbsenceDays = authorized,
                    UnauthorizedAbsenceDays = absentDays - authorized,
                    AbsenceRate = absenceRate,
                    AbsenceReasons = reasons
                };
            })
            .OrderByDescending(r => r.AbsentDays)
            .ToList();

        // Department summary
        var deptSummary = rows
            .GroupBy(r => r.Department)
            .Select(g =>
            {
                // Count distinct employees in this department from original absence records
                var deptEmployeeCount = absences
                    .Where(a => (a.Employee.Department?.NameEn ?? string.Empty) == g.Key)
                    .Select(a => a.EmployeeId)
                    .Distinct()
                    .Count();

                var totalAbsent = g.Sum(r => r.AbsentDays);
                var possibleDays = deptEmployeeCount * totalDaysInRange;
                var rate = possibleDays > 0 ? Math.Round((double)totalAbsent / possibleDays * 100, 2) : 0;

                return new DepartmentAbsenceSummaryDto
                {
                    DepartmentName = g.Key,
                    TotalEmployees = deptEmployeeCount,
                    TotalAbsentDays = totalAbsent,
                    AbsenceRate = rate
                };
            })
            .OrderByDescending(d => d.AbsenceRate)
            .ToList();

        var totalAbsentDays = rows.Sum(r => r.AbsentDays);
        var allEmployees = rows.Select(r => r.EmployeeCode).Distinct().Count();
        var overallRate = allEmployees > 0 && totalDaysInRange > 0
            ? Math.Round((double)totalAbsentDays / (allEmployees * totalDaysInRange) * 100, 2)
            : 0;

        var dto = new AbsenteeismReportDto
        {
            Meta = new AttendanceReportMeta
            {
                ReportType = "Absenteeism Report",
                GeneratedAt = DateTime.UtcNow,
                FromDate = from,
                ToDate = to
            },
            TotalAbsentDays = totalAbsentDays,
            OverallAbsenceRate = overallRate,
            Rows = rows,
            DepartmentSummary = deptSummary
        };

        return GenericResponse<AbsenteeismReportDto>.SuccessResult(dto, "Absenteeism report generated successfully.");
    }
}
