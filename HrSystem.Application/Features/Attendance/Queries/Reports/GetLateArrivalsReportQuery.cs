using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Attendance.Queries.Reports;

public record GetLateArrivalsReportQuery(
    DateTime FromDate,
    DateTime ToDate,
    Guid? DepartmentId = null,
    Guid? EmployeeId = null
) : IRequest<ErrorOr<GenericResponse<LateArrivalsReportDto>>>;

public class GetLateArrivalsReportQueryHandler
    : IRequestHandler<GetLateArrivalsReportQuery, ErrorOr<GenericResponse<LateArrivalsReportDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetLateArrivalsReportQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<LateArrivalsReportDto>>> Handle(
        GetLateArrivalsReportQuery request,
        CancellationToken cancellationToken)
    {
        var from = request.FromDate.Date;
        var to = request.ToDate.Date;

        if (from > to)
            return Error.Validation(description: "FromDate must be before or equal to ToDate.");

        var isSuperOrOrgAdmin = CurrentUser.IsOrganizationAdmin || CurrentUser.IsSuperAdmin;
        var branchId = isSuperOrOrgAdmin ? (Guid?)null : CurrentUser.BranchId;

        // Get branch work schedule to determine scheduled start times
        Domain.Entities.Organization.BranchWorkSchedule? schedule = null;
        if (branchId.HasValue)
        {
            schedule = await _context.BranchWorkSchedules.AsNoTracking()
                .FirstOrDefaultAsync(s => s.BranchId == branchId.Value && s.IsActive && !s.IsDeleted, cancellationToken);
        }

        var query = _context.Attendances
            .AsNoTracking()
            .Include(a => a.Employee)
                .ThenInclude(e => e.Department)
            .Where(a => !a.IsDeleted
                && !a.IsConfigurationRecord
                && a.IsLate
                && a.Date.Date >= from
                && a.Date.Date <= to);

        if (branchId.HasValue)
            query = query.Where(a => a.Employee.BranchId == branchId);

        if (request.DepartmentId.HasValue)
            query = query.Where(a => a.Employee.DepartmentId == request.DepartmentId);

        if (request.EmployeeId.HasValue)
            query = query.Where(a => a.EmployeeId == request.EmployeeId);

        var records = await query
            .OrderBy(a => a.Date)
            .ThenBy(a => a.Employee.FirstNameEn)
            .ToListAsync(cancellationToken);

        var rows = records.Select(a => new LateArrivalRowDto
        {
            EmployeeCode = a.Employee.EmployeeCode,
            EmployeeName = a.Employee.FirstNameEn + " " + a.Employee.LastNameEn,
            Department = a.Employee.Department?.NameEn ?? string.Empty,
            Date = a.Date,
            ScheduledStartTime = schedule?.StartTime,
            ActualCheckIn = a.CheckInTime,
            MinutesLate = a.LateMinutes.HasValue ? (int)a.LateMinutes.Value.TotalMinutes : 0
        }).ToList();

        // Per-employee summary
        var summary = records
            .GroupBy(a => new
            {
                a.EmployeeId,
                Code = a.Employee.EmployeeCode,
                Name = a.Employee.FirstNameEn + " " + a.Employee.LastNameEn,
                Dept = a.Employee.Department?.NameEn ?? string.Empty
            })
            .Select(g =>
            {
                var freq = g.Count();
                var totalMin = g.Sum(a => a.LateMinutes.HasValue ? a.LateMinutes.Value.TotalMinutes : 0);
                return new LateArrivalEmployeeSummaryDto
                {
                    EmployeeCode = g.Key.Code,
                    EmployeeName = g.Key.Name,
                    Department = g.Key.Dept,
                    LateFrequency = freq,
                    TotalLateMinutes = Math.Round(totalMin, 1),
                    AverageLateMinutes = freq > 0 ? Math.Round(totalMin / freq, 1) : 0
                };
            })
            .OrderByDescending(s => s.LateFrequency)
            .ToList();

        var dto = new LateArrivalsReportDto
        {
            Meta = new AttendanceReportMeta
            {
                ReportType = "Late Arrivals Report",
                GeneratedAt = DateTime.UtcNow,
                FromDate = from,
                ToDate = to
            },
            TotalLateInstances = records.Count,
            Rows = rows,
            EmployeeSummary = summary
        };

        return GenericResponse<LateArrivalsReportDto>.SuccessResult(dto, "Late arrivals report generated successfully.");
    }
}
