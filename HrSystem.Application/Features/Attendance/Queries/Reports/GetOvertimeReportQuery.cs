using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Attendance.Queries.Reports;

public record GetOvertimeReportQuery(
    DateTime FromDate,
    DateTime ToDate,
    Guid? DepartmentId = null,
    Guid? EmployeeId = null
) : IRequest<ErrorOr<GenericResponse<OvertimeReportDto>>>;

public class GetOvertimeReportQueryHandler
    : IRequestHandler<GetOvertimeReportQuery, ErrorOr<GenericResponse<OvertimeReportDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetOvertimeReportQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<OvertimeReportDto>>> Handle(
        GetOvertimeReportQuery request,
        CancellationToken cancellationToken)
    {
        var from = request.FromDate.Date;
        var to = request.ToDate.Date;

        if (from > to)
            return Error.Validation(description: "FromDate must be before or equal to ToDate.");

        var isSuperOrOrgAdmin = CurrentUser.IsOrganizationAdmin || CurrentUser.IsSuperAdmin;
        var branchId = isSuperOrOrgAdmin ? (Guid?)null : CurrentUser.BranchId;

        var query = _context.Attendances
            .AsNoTracking()
            .Include(a => a.Employee)
                .ThenInclude(e => e.Department)
            .Include(a => a.Employee)
                .ThenInclude(e => e.Salaries)
            .Where(a => !a.IsDeleted
                && !a.IsConfigurationRecord
                && a.IsOvertime
                && a.OvertimeHours != null
                && a.Date.Date >= from
                && a.Date.Date <= to);

        if (branchId.HasValue)
            query = query.Where(a => a.Employee.BranchId == branchId);

        if (request.DepartmentId.HasValue)
            query = query.Where(a => a.Employee.DepartmentId == request.DepartmentId);

        if (request.EmployeeId.HasValue)
            query = query.Where(a => a.EmployeeId == request.EmployeeId);

        var records = await query.ToListAsync(cancellationToken);

        var rows = records
            .GroupBy(a => new
            {
                a.EmployeeId,
                Code = a.Employee.EmployeeCode,
                Name = a.Employee.FirstNameEn + " " + a.Employee.LastNameEn,
                Dept = a.Employee.Department?.NameEn ?? string.Empty
            })
            .Select(g =>
            {
                var currentSalary = g.First().Employee.Salaries
                    .Where(s => s.IsCurrent && !s.IsDeleted)
                    .OrderByDescending(s => s.EffectiveDate)
                    .FirstOrDefault();

                var basicSalary = currentSalary?.BasicSalary ?? 0m;
                var multiplier = currentSalary?.OvertimeMultiplier ?? 1.5m;

                // Assume 22 working days per month × 8 hours for hourly rate
                var hourlyRate = basicSalary > 0 ? basicSalary / 22m / 8m : 0m;

                var regularHours = g.Sum(a => a.WorkedHours.HasValue
                    ? a.WorkedHours.Value.TotalHours - (a.OvertimeHours?.TotalHours ?? 0)
                    : 0);
                var overtimeHours = g.Sum(a => a.OvertimeHours.HasValue ? a.OvertimeHours.Value.TotalHours : 0);
                var otCost = (decimal)overtimeHours * hourlyRate * multiplier;

                return new OvertimeRowDto
                {
                    EmployeeCode = g.Key.Code,
                    EmployeeName = g.Key.Name,
                    Department = g.Key.Dept,
                    BasicSalary = basicSalary,
                    RegularHours = Math.Round(Math.Max(regularHours, 0), 2),
                    OvertimeHours = Math.Round(overtimeHours, 2),
                    OvertimeMultiplier = multiplier,
                    HourlyRate = Math.Round(hourlyRate, 2),
                    OvertimeCost = Math.Round(otCost, 2)
                };
            })
            .OrderBy(r => r.Department).ThenBy(r => r.EmployeeName)
            .ToList();

        var deptSummary = rows
            .GroupBy(r => r.Department)
            .Select(g => new DepartmentOvertimeSummaryDto
            {
                DepartmentName = g.Key,
                TotalOvertimeHours = Math.Round(g.Sum(r => r.OvertimeHours), 2),
                TotalOvertimeCost = Math.Round(g.Sum(r => r.OvertimeCost), 2)
            })
            .OrderByDescending(d => d.TotalOvertimeCost)
            .ToList();

        var dto = new OvertimeReportDto
        {
            Meta = new AttendanceReportMeta
            {
                ReportType = "Overtime Report",
                GeneratedAt = DateTime.UtcNow,
                FromDate = from,
                ToDate = to
            },
            TotalOvertimeHours = Math.Round(rows.Sum(r => r.OvertimeHours), 2),
            TotalOvertimeCost = Math.Round(rows.Sum(r => r.OvertimeCost), 2),
            Rows = rows,
            DepartmentSummary = deptSummary
        };

        return GenericResponse<OvertimeReportDto>.SuccessResult(dto, "Overtime report generated successfully.");
    }
}
