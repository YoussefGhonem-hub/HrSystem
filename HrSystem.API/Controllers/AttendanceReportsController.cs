using HrSystem.API.Controllers.Shared;
using HrSystem.Application.Features.Attendance.Queries.Reports;
using HrSystem.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrSystem.API.Controllers;

/// <summary>
/// Provides attendance reporting endpoints for HR, Payroll, and Management.
/// Each endpoint is role-gated and supports JSON, Excel (.xlsx), or PDF output
/// via the optional <c>format</c> query parameter (json | excel | pdf).
/// </summary>
[Route("api/attendance-reports")]
[Authorize]
public class AttendanceReportsController : APIBaseController
{
    private readonly ISender _mediator;

    public AttendanceReportsController(ISender mediator) => _mediator = mediator;

    // ══════════════════════════════════════════════════════════════════
    //  1. Daily Attendance Report
    //  Roles: HRManager, HRSpecialist, DepartmentManager, OrganizationAdmin
    // ══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Daily attendance report. Returns real-time visibility of employee attendance for a given date.
    /// Defaults to today when <paramref name="date"/> is not provided.
    /// </summary>
    [HttpGet("daily")]
    [Authorize(Roles =
        RoleNames.HRManager + "," +
        RoleNames.HRSpecialist + "," +
        RoleNames.DepartmentManager + "," +
        RoleNames.OrganizationAdmin)]
    public async Task<IActionResult> GetDailyAttendanceReport(
        [FromQuery] DateTime? date = null,
        [FromQuery] Guid? departmentId = null,
        [FromQuery] Guid? employeeId = null,
        [FromQuery] string format = "json")
    {
        var result = await _mediator.Send(new GetDailyAttendanceReportQuery(date, departmentId, employeeId));

        return result.Match(
            response =>
            {
                if (format.Equals("excel", StringComparison.OrdinalIgnoreCase))
                {
                    var bytes = AttendanceReportExcelExporter.ExportDailyAttendance(response.Data!);
                    return File(bytes,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"daily-attendance-{response.Data!.ReportDate:yyyy-MM-dd}.xlsx");
                }
                if (format.Equals("pdf", StringComparison.OrdinalIgnoreCase))
                {
                    var bytes = AttendanceReportPdfExporter.ExportDailyAttendance(response.Data!);
                    return File(bytes, "application/pdf",
                        $"daily-attendance-{response.Data!.ReportDate:yyyy-MM-dd}.pdf");
                }
                return Ok(response);
            },
            errors => Problem(errors)
        );
    }

    // ══════════════════════════════════════════════════════════════════
    //  2. Monthly Attendance Summary Report
    //  Roles: HRManager, HRSpecialist, DepartmentManager, OrganizationAdmin
    // ══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Full monthly attendance performance summary per employee.
    /// </summary>
    [HttpGet("monthly-summary")]
    [Authorize(Roles =
        RoleNames.HRManager + "," +
        RoleNames.HRSpecialist + "," +
        RoleNames.DepartmentManager + "," +
        RoleNames.OrganizationAdmin)]
    public async Task<IActionResult> GetMonthlyAttendanceSummaryReport(
        [FromQuery] int? month = null,
        [FromQuery] int? year = null,
        [FromQuery] Guid? departmentId = null,
        [FromQuery] Guid? employeeId = null,
        [FromQuery] string format = "json")
    {
        var now = DateTime.UtcNow;
        var effectiveMonth = month ?? now.Month;
        var effectiveYear = year ?? now.Year;

        var result = await _mediator.Send(
            new GetMonthlyAttendanceSummaryReportQuery(effectiveMonth, effectiveYear, departmentId, employeeId));

        return result.Match(
            response =>
            {
                if (format.Equals("excel", StringComparison.OrdinalIgnoreCase))
                {
                    var bytes = AttendanceReportExcelExporter.ExportMonthlySummary(response.Data!);
                    return File(bytes,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"monthly-summary-{effectiveYear}-{effectiveMonth:D2}.xlsx");
                }
                if (format.Equals("pdf", StringComparison.OrdinalIgnoreCase))
                {
                    var bytes = AttendanceReportPdfExporter.ExportMonthlySummary(response.Data!);
                    return File(bytes, "application/pdf",
                        $"monthly-summary-{effectiveYear}-{effectiveMonth:D2}.pdf");
                }
                return Ok(response);
            },
            errors => Problem(errors)
        );
    }

    // ══════════════════════════════════════════════════════════════════
    //  3. Payroll Attendance Report
    //  Roles: HRManager, OrganizationAdmin  (Payroll team only)
    // ══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Payroll attendance report with deduction and overtime amounts.
    /// Restricted to Payroll team (HRManager / OrganizationAdmin).
    /// </summary>
    [HttpGet("payroll")]
    [Authorize(Roles = RoleNames.HRManager + "," + RoleNames.OrganizationAdmin)]
    public async Task<IActionResult> GetPayrollAttendanceReport(
        [FromQuery] int? month = null,
        [FromQuery] int? year = null,
        [FromQuery] Guid? departmentId = null,
        [FromQuery] Guid? employeeId = null,
        [FromQuery] string format = "json")
    {
        var now = DateTime.UtcNow;
        var effectiveMonth = month ?? now.Month;
        var effectiveYear = year ?? now.Year;

        var result = await _mediator.Send(
            new GetPayrollAttendanceReportQuery(effectiveMonth, effectiveYear, departmentId, employeeId));

        return result.Match(
            response =>
            {
                if (format.Equals("excel", StringComparison.OrdinalIgnoreCase))
                {
                    var bytes = AttendanceReportExcelExporter.ExportPayrollAttendance(response.Data!);
                    return File(bytes,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"payroll-attendance-{effectiveYear}-{effectiveMonth:D2}.xlsx");
                }
                if (format.Equals("pdf", StringComparison.OrdinalIgnoreCase))
                {
                    var bytes = AttendanceReportPdfExporter.ExportPayrollAttendance(response.Data!);
                    return File(bytes, "application/pdf",
                        $"payroll-attendance-{effectiveYear}-{effectiveMonth:D2}.pdf");
                }
                return Ok(response);
            },
            errors => Problem(errors)
        );
    }

    // ══════════════════════════════════════════════════════════════════
    //  4. Late Arrivals Report
    //  Roles: HRManager, HRSpecialist, DepartmentManager, OrganizationAdmin
    // ══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Late arrivals report with per-employee punctuality trends.
    /// </summary>
    [HttpGet("late-arrivals")]
    [Authorize(Roles =
        RoleNames.HRManager + "," +
        RoleNames.HRSpecialist + "," +
        RoleNames.DepartmentManager + "," +
        RoleNames.OrganizationAdmin)]
    public async Task<IActionResult> GetLateArrivalsReport(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] Guid? departmentId = null,
        [FromQuery] Guid? employeeId = null,
        [FromQuery] string format = "json")
    {
        var now = DateTime.UtcNow;
        var from = fromDate ?? now.AddDays(-30);
        var to = toDate ?? now;

        var result = await _mediator.Send(new GetLateArrivalsReportQuery(from, to, departmentId, employeeId));

        return result.Match(
            response =>
            {
                if (format.Equals("excel", StringComparison.OrdinalIgnoreCase))
                {
                    var bytes = AttendanceReportExcelExporter.ExportLateArrivals(response.Data!);
                    return File(bytes,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"late-arrivals-{from:yyyy-MM-dd}-to-{to:yyyy-MM-dd}.xlsx");
                }
                if (format.Equals("pdf", StringComparison.OrdinalIgnoreCase))
                {
                    var bytes = AttendanceReportPdfExporter.ExportLateArrivals(response.Data!);
                    return File(bytes, "application/pdf",
                        $"late-arrivals-{from:yyyy-MM-dd}-to-{to:yyyy-MM-dd}.pdf");
                }
                return Ok(response);
            },
            errors => Problem(errors)
        );
    }

    // ══════════════════════════════════════════════════════════════════
    //  5. Absenteeism Report
    //  Roles: HRManager, OrganizationAdmin
    // ══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Absenteeism report analyzing absence patterns, reasons, and risks.
    /// </summary>
    [HttpGet("absenteeism")]
    [Authorize(Roles = RoleNames.HRManager + "," + RoleNames.OrganizationAdmin)]
    public async Task<IActionResult> GetAbsenteeismReport(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] Guid? departmentId = null,
        [FromQuery] Guid? employeeId = null,
        [FromQuery] string format = "json")
    {
        var now = DateTime.UtcNow;
        var firstOfMonth = new DateTime(now.Year, now.Month, 1);
        var from = fromDate ?? firstOfMonth;
        var to = toDate ?? now;

        var result = await _mediator.Send(new GetAbsenteeismReportQuery(from, to, departmentId, employeeId));

        return result.Match(
            response =>
            {
                if (format.Equals("excel", StringComparison.OrdinalIgnoreCase))
                {
                    var bytes = AttendanceReportExcelExporter.ExportAbsenteeism(response.Data!);
                    return File(bytes,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"absenteeism-{from:yyyy-MM-dd}-to-{to:yyyy-MM-dd}.xlsx");
                }
                if (format.Equals("pdf", StringComparison.OrdinalIgnoreCase))
                {
                    var bytes = AttendanceReportPdfExporter.ExportAbsenteeism(response.Data!);
                    return File(bytes, "application/pdf",
                        $"absenteeism-{from:yyyy-MM-dd}-to-{to:yyyy-MM-dd}.pdf");
                }
                return Ok(response);
            },
            errors => Problem(errors)
        );
    }

    // ══════════════════════════════════════════════════════════════════
    //  6. Overtime Report
    //  Roles: HRManager, HRSpecialist, DepartmentManager, OrganizationAdmin
    // ══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Overtime report tracking usage and cost impact per employee and department.
    /// </summary>
    [HttpGet("overtime")]
    [Authorize(Roles =
        RoleNames.HRManager + "," +
        RoleNames.HRSpecialist + "," +
        RoleNames.DepartmentManager + "," +
        RoleNames.OrganizationAdmin)]
    public async Task<IActionResult> GetOvertimeReport(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] Guid? departmentId = null,
        [FromQuery] Guid? employeeId = null,
        [FromQuery] string format = "json")
    {
        var now = DateTime.UtcNow;
        var firstOfMonth = new DateTime(now.Year, now.Month, 1);
        var from = fromDate ?? firstOfMonth;
        var to = toDate ?? now;

        var result = await _mediator.Send(new GetOvertimeReportQuery(from, to, departmentId, employeeId));

        return result.Match(
            response =>
            {
                if (format.Equals("excel", StringComparison.OrdinalIgnoreCase))
                {
                    var bytes = AttendanceReportExcelExporter.ExportOvertime(response.Data!);
                    return File(bytes,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"overtime-{from:yyyy-MM-dd}-to-{to:yyyy-MM-dd}.xlsx");
                }
                if (format.Equals("pdf", StringComparison.OrdinalIgnoreCase))
                {
                    var bytes = AttendanceReportPdfExporter.ExportOvertime(response.Data!);
                    return File(bytes, "application/pdf",
                        $"overtime-{from:yyyy-MM-dd}-to-{to:yyyy-MM-dd}.pdf");
                }
                return Ok(response);
            },
            errors => Problem(errors)
        );
    }

    // ══════════════════════════════════════════════════════════════════
    //  7. Department Attendance Report
    //  Roles: HRManager, OrganizationAdmin
    // ══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Cross-department attendance comparison — attendance %, absent days, late instances, overtime.
    /// </summary>
    [HttpGet("departments")]
    [Authorize(Roles = RoleNames.HRManager + "," + RoleNames.OrganizationAdmin)]
    public async Task<IActionResult> GetDepartmentAttendanceReport(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] Guid? departmentId = null,
        [FromQuery] string format = "json")
    {
        var now = DateTime.UtcNow;
        var firstOfMonth = new DateTime(now.Year, now.Month, 1);
        var from = fromDate ?? firstOfMonth;
        var to = toDate ?? now;

        var result = await _mediator.Send(new GetDepartmentAttendanceReportQuery(from, to, departmentId));

        return result.Match(
            response =>
            {
                if (format.Equals("excel", StringComparison.OrdinalIgnoreCase))
                {
                    var bytes = AttendanceReportExcelExporter.ExportDepartmentAttendance(response.Data!);
                    return File(bytes,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"department-attendance-{from:yyyy-MM-dd}-to-{to:yyyy-MM-dd}.xlsx");
                }
                if (format.Equals("pdf", StringComparison.OrdinalIgnoreCase))
                {
                    var bytes = AttendanceReportPdfExporter.ExportDepartmentAttendance(response.Data!);
                    return File(bytes, "application/pdf",
                        $"department-attendance-{from:yyyy-MM-dd}-to-{to:yyyy-MM-dd}.pdf");
                }
                return Ok(response);
            },
            errors => Problem(errors)
        );
    }
}
