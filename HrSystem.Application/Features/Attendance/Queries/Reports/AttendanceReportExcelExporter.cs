using ClosedXML.Excel;

namespace HrSystem.Application.Features.Attendance.Queries.Reports;

/// <summary>
/// Converts attendance report DTOs into Excel workbooks (.xlsx) using ClosedXML.
/// Each public static method corresponds to one of the 7 report types.
/// </summary>
public static class AttendanceReportExcelExporter
{
    // ─── Shared styling constants ───────────────────────────────────────────
    private static readonly XLColor HeaderBg = XLColor.FromHtml("#1F3864");
    private static readonly XLColor SummaryBg = XLColor.FromHtml("#D6E4F7");
    private static readonly XLColor AlternateRow = XLColor.FromHtml("#F5F9FF");

    // ── 1. Daily Attendance ──────────────────────────────────────────────────
    public static byte[] ExportDailyAttendance(DailyAttendanceReportDto report)
    {
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Daily Attendance");

        AddReportTitle(ws, $"Daily Attendance Report — {report.ReportDate:dd MMM yyyy}", 9);

        // Summary row
        int row = 3;
        ws.Cell(row, 1).Value = $"Total: {report.TotalEmployees}  |  Present: {report.PresentCount}  |  Absent: {report.AbsentCount}  |  Late: {report.LateCount}  |  Early Leave: {report.EarlyLeaveCount}  |  On Leave: {report.OnLeaveCount}  |  Attendance Rate: {report.AttendanceRate}%";
        ws.Range(row, 1, row, 9).Merge().Style.Fill.BackgroundColor = SummaryBg;
        row += 2;

        // Header
        string[] headers = ["Employee Code", "Employee Name", "Department", "Job Title", "Status", "Check-In", "Check-Out", "Late (min)", "Worked Hours"];
        AddHeaders(ws, row, headers);
        row++;

        foreach (var r in report.Rows)
        {
            ws.Cell(row, 1).Value = r.EmployeeCode;
            ws.Cell(row, 2).Value = r.EmployeeName;
            ws.Cell(row, 3).Value = r.Department;
            ws.Cell(row, 4).Value = r.JobTitle;
            ws.Cell(row, 5).Value = r.Status;
            ws.Cell(row, 6).Value = r.CheckIn.HasValue ? r.CheckIn.Value.ToString(@"hh\:mm") : "—";
            ws.Cell(row, 7).Value = r.CheckOut.HasValue ? r.CheckOut.Value.ToString(@"hh\:mm") : "—";
            ws.Cell(row, 8).Value = r.LateMinutes;
            ws.Cell(row, 9).Value = r.WorkedHours;
            if (row % 2 == 0) ApplyAlternateRowStyle(ws, row, 9);
            row++;
        }

        AutoFitColumns(ws, 9);
        return ToBytes(wb);
    }

    // ── 2. Monthly Attendance Summary ────────────────────────────────────────
    public static byte[] ExportMonthlySummary(MonthlyAttendanceSummaryReportDto report)
    {
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Monthly Summary");

        var monthName = new DateTime(report.Year, report.Month, 1).ToString("MMMM yyyy");
        AddReportTitle(ws, $"Monthly Attendance Summary — {monthName}", 10);

        int row = 3;
        ws.Cell(row, 1).Value = $"Total Working Days: {report.TotalWorkingDays}";
        ws.Range(row, 1, row, 10).Merge().Style.Fill.BackgroundColor = SummaryBg;
        row += 2;

        string[] headers = ["Code", "Employee Name", "Department", "Job Title", "Days Present", "Days Absent", "Days Late", "Late (min)", "OT Hours", "Leave Days", "Attendance %"];
        AddHeaders(ws, row, headers);
        row++;

        foreach (var r in report.Rows)
        {
            ws.Cell(row, 1).Value = r.EmployeeCode;
            ws.Cell(row, 2).Value = r.EmployeeName;
            ws.Cell(row, 3).Value = r.Department;
            ws.Cell(row, 4).Value = r.JobTitle;
            ws.Cell(row, 5).Value = r.DaysPresent;
            ws.Cell(row, 6).Value = r.DaysAbsent;
            ws.Cell(row, 7).Value = r.DaysLate;
            ws.Cell(row, 8).Value = r.TotalLateMinutes;
            ws.Cell(row, 9).Value = r.OvertimeHours;
            ws.Cell(row, 10).Value = r.LeaveDaysTaken;
            ws.Cell(row, 11).Value = r.AttendancePercentage;
            if (row % 2 == 0) ApplyAlternateRowStyle(ws, row, 11);
            row++;
        }

        AutoFitColumns(ws, 11);
        return ToBytes(wb);
    }

    // ── 3. Payroll Attendance ────────────────────────────────────────────────
    public static byte[] ExportPayrollAttendance(PayrollAttendanceReportDto report)
    {
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Payroll Attendance");

        var monthName = new DateTime(report.Year, report.Month, 1).ToString("MMMM yyyy");
        AddReportTitle(ws, $"Payroll Attendance Report — {monthName}", 12);

        int row = 3;
        ws.Cell(row, 1).Value = $"Working Days: {report.TotalWorkingDays}  |  Total Late Deductions: {report.TotalLateDeductions:N2}  |  Total OT Amount: {report.TotalOvertimeAmounts:N2}";
        ws.Range(row, 1, row, 12).Merge().Style.Fill.BackgroundColor = SummaryBg;
        row += 2;

        string[] headers = ["Code", "Employee", "Department", "Basic Salary", "Working Days", "Worked Days", "Absent Days", "Late (min)", "Daily Rate", "Absent Deduction", "Late Deduction", "OT Hours", "OT Amount", "Net Impact"];
        AddHeaders(ws, row, headers);
        row++;

        foreach (var r in report.Rows)
        {
            ws.Cell(row, 1).Value = r.EmployeeCode;
            ws.Cell(row, 2).Value = r.EmployeeName;
            ws.Cell(row, 3).Value = r.Department;
            ws.Cell(row, 4).Value = r.BasicSalary;
            ws.Cell(row, 5).Value = r.TotalWorkingDays;
            ws.Cell(row, 6).Value = r.WorkedDays;
            ws.Cell(row, 7).Value = r.AbsentDays;
            ws.Cell(row, 8).Value = r.LateMinutes;
            ws.Cell(row, 9).Value = r.DailyRate;
            ws.Cell(row, 10).Value = r.AbsentDeductionAmount;
            ws.Cell(row, 11).Value = r.LateDeductionAmount;
            ws.Cell(row, 12).Value = r.OvertimeHours;
            ws.Cell(row, 13).Value = r.OvertimeAmount;
            ws.Cell(row, 14).Value = r.NetSalaryImpact;
            if (row % 2 == 0) ApplyAlternateRowStyle(ws, row, 14);
            row++;
        }

        AutoFitColumns(ws, 14);
        return ToBytes(wb);
    }

    // ── 4. Late Arrivals ─────────────────────────────────────────────────────
    public static byte[] ExportLateArrivals(LateArrivalsReportDto report)
    {
        using var wb = new XLWorkbook();
        var wsDetail = wb.AddWorksheet("Late Arrivals Detail");
        var wsSummary = wb.AddWorksheet("Employee Summary");

        AddReportTitle(wsDetail, $"Late Arrivals Report — {report.Meta.FromDate:dd MMM yyyy} to {report.Meta.ToDate:dd MMM yyyy}", 7);
        int row = 3;
        wsDetail.Cell(row, 1).Value = $"Total Late Instances: {report.TotalLateInstances}";
        wsDetail.Range(row, 1, row, 7).Merge().Style.Fill.BackgroundColor = SummaryBg;
        row += 2;

        AddHeaders(wsDetail, row, ["Code", "Employee Name", "Department", "Date", "Scheduled Time", "Actual Check-In", "Minutes Late"]);
        row++;
        foreach (var r in report.Rows)
        {
            wsDetail.Cell(row, 1).Value = r.EmployeeCode;
            wsDetail.Cell(row, 2).Value = r.EmployeeName;
            wsDetail.Cell(row, 3).Value = r.Department;
            wsDetail.Cell(row, 4).Value = r.Date.ToString("dd/MM/yyyy");
            wsDetail.Cell(row, 5).Value = r.ScheduledStartTime.HasValue ? r.ScheduledStartTime.Value.ToString(@"hh\:mm") : "—";
            wsDetail.Cell(row, 6).Value = r.ActualCheckIn.HasValue ? r.ActualCheckIn.Value.ToString(@"hh\:mm") : "—";
            wsDetail.Cell(row, 7).Value = r.MinutesLate;
            if (row % 2 == 0) ApplyAlternateRowStyle(wsDetail, row, 7);
            row++;
        }
        AutoFitColumns(wsDetail, 7);

        // Summary sheet
        AddReportTitle(wsSummary, "Late Arrivals — Employee Summary", 6);
        row = 5;
        AddHeaders(wsSummary, row, ["Code", "Employee Name", "Department", "Frequency", "Total Late (min)", "Avg Late (min)"]);
        row++;
        foreach (var s in report.EmployeeSummary)
        {
            wsSummary.Cell(row, 1).Value = s.EmployeeCode;
            wsSummary.Cell(row, 2).Value = s.EmployeeName;
            wsSummary.Cell(row, 3).Value = s.Department;
            wsSummary.Cell(row, 4).Value = s.LateFrequency;
            wsSummary.Cell(row, 5).Value = s.TotalLateMinutes;
            wsSummary.Cell(row, 6).Value = s.AverageLateMinutes;
            if (row % 2 == 0) ApplyAlternateRowStyle(wsSummary, row, 6);
            row++;
        }
        AutoFitColumns(wsSummary, 6);

        return ToBytes(wb);
    }

    // ── 5. Absenteeism ───────────────────────────────────────────────────────
    public static byte[] ExportAbsenteeism(AbsenteeismReportDto report)
    {
        using var wb = new XLWorkbook();
        var wsEmp = wb.AddWorksheet("Employee Absenteeism");
        var wsDept = wb.AddWorksheet("Department Summary");

        AddReportTitle(wsEmp, $"Absenteeism Report — {report.Meta.FromDate:dd MMM yyyy} to {report.Meta.ToDate:dd MMM yyyy}", 7);
        int row = 3;
        wsEmp.Cell(row, 1).Value = $"Total Absent Days: {report.TotalAbsentDays}  |  Overall Absence Rate: {report.OverallAbsenceRate}%";
        wsEmp.Range(row, 1, row, 7).Merge().Style.Fill.BackgroundColor = SummaryBg;
        row += 2;

        AddHeaders(wsEmp, row, ["Code", "Employee Name", "Department", "Absent Days", "Authorized", "Unauthorized", "Absence Rate %", "Reasons"]);
        row++;
        foreach (var r in report.Rows)
        {
            wsEmp.Cell(row, 1).Value = r.EmployeeCode;
            wsEmp.Cell(row, 2).Value = r.EmployeeName;
            wsEmp.Cell(row, 3).Value = r.Department;
            wsEmp.Cell(row, 4).Value = r.AbsentDays;
            wsEmp.Cell(row, 5).Value = r.AuthorizedAbsenceDays;
            wsEmp.Cell(row, 6).Value = r.UnauthorizedAbsenceDays;
            wsEmp.Cell(row, 7).Value = r.AbsenceRate;
            wsEmp.Cell(row, 8).Value = string.Join(", ", r.AbsenceReasons);
            if (row % 2 == 0) ApplyAlternateRowStyle(wsEmp, row, 8);
            row++;
        }
        AutoFitColumns(wsEmp, 8);

        // Department sheet
        AddReportTitle(wsDept, "Absenteeism — Department Summary", 4);
        row = 5;
        AddHeaders(wsDept, row, ["Department", "Total Employees", "Total Absent Days", "Absence Rate %"]);
        row++;
        foreach (var d in report.DepartmentSummary)
        {
            wsDept.Cell(row, 1).Value = d.DepartmentName;
            wsDept.Cell(row, 2).Value = d.TotalEmployees;
            wsDept.Cell(row, 3).Value = d.TotalAbsentDays;
            wsDept.Cell(row, 4).Value = d.AbsenceRate;
            if (row % 2 == 0) ApplyAlternateRowStyle(wsDept, row, 4);
            row++;
        }
        AutoFitColumns(wsDept, 4);

        return ToBytes(wb);
    }

    // ── 6. Overtime ──────────────────────────────────────────────────────────
    public static byte[] ExportOvertime(OvertimeReportDto report)
    {
        using var wb = new XLWorkbook();
        var wsEmp = wb.AddWorksheet("Overtime Detail");
        var wsDept = wb.AddWorksheet("Department Summary");

        AddReportTitle(wsEmp, $"Overtime Report — {report.Meta.FromDate:dd MMM yyyy} to {report.Meta.ToDate:dd MMM yyyy}", 8);
        int row = 3;
        wsEmp.Cell(row, 1).Value = $"Total OT Hours: {report.TotalOvertimeHours}  |  Total OT Cost: {report.TotalOvertimeCost:N2}";
        wsEmp.Range(row, 1, row, 8).Merge().Style.Fill.BackgroundColor = SummaryBg;
        row += 2;

        AddHeaders(wsEmp, row, ["Code", "Employee", "Department", "Regular Hrs", "OT Hours", "Basic Salary", "OT Multiplier", "Hourly Rate", "OT Cost"]);
        row++;
        foreach (var r in report.Rows)
        {
            wsEmp.Cell(row, 1).Value = r.EmployeeCode;
            wsEmp.Cell(row, 2).Value = r.EmployeeName;
            wsEmp.Cell(row, 3).Value = r.Department;
            wsEmp.Cell(row, 4).Value = r.RegularHours;
            wsEmp.Cell(row, 5).Value = r.OvertimeHours;
            wsEmp.Cell(row, 6).Value = r.BasicSalary;
            wsEmp.Cell(row, 7).Value = r.OvertimeMultiplier;
            wsEmp.Cell(row, 8).Value = r.HourlyRate;
            wsEmp.Cell(row, 9).Value = r.OvertimeCost;
            if (row % 2 == 0) ApplyAlternateRowStyle(wsEmp, row, 9);
            row++;
        }
        AutoFitColumns(wsEmp, 9);

        // Department sheet
        AddReportTitle(wsDept, "Overtime — Department Summary", 3);
        row = 5;
        AddHeaders(wsDept, row, ["Department", "Total OT Hours", "Total OT Cost"]);
        row++;
        foreach (var d in report.DepartmentSummary)
        {
            wsDept.Cell(row, 1).Value = d.DepartmentName;
            wsDept.Cell(row, 2).Value = d.TotalOvertimeHours;
            wsDept.Cell(row, 3).Value = d.TotalOvertimeCost;
            if (row % 2 == 0) ApplyAlternateRowStyle(wsDept, row, 3);
            row++;
        }
        AutoFitColumns(wsDept, 3);

        return ToBytes(wb);
    }

    // ── 7. Department Attendance ─────────────────────────────────────────────
    public static byte[] ExportDepartmentAttendance(DepartmentAttendanceReportDto report)
    {
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Department Attendance");

        AddReportTitle(ws, $"Department Attendance Report — {report.Meta.FromDate:dd MMM yyyy} to {report.Meta.ToDate:dd MMM yyyy}", 6);
        int row = 3;
        ws.Cell(row, 1).Value = $"Total Working Days: {report.TotalWorkingDays}";
        ws.Range(row, 1, row, 6).Merge().Style.Fill.BackgroundColor = SummaryBg;
        row += 2;

        AddHeaders(ws, row, ["Department", "Total Employees", "Avg Attendance %", "Total Absent Days", "Total Late Instances", "Total OT Hours"]);
        row++;
        foreach (var r in report.Rows)
        {
            ws.Cell(row, 1).Value = r.DepartmentName;
            ws.Cell(row, 2).Value = r.TotalEmployees;
            ws.Cell(row, 3).Value = r.AverageAttendancePercentage;
            ws.Cell(row, 4).Value = r.TotalAbsentDays;
            ws.Cell(row, 5).Value = r.TotalLateInstances;
            ws.Cell(row, 6).Value = r.TotalOvertimeHours;
            if (row % 2 == 0) ApplyAlternateRowStyle(ws, row, 6);
            row++;
        }

        AutoFitColumns(ws, 6);
        return ToBytes(wb);
    }

    // ─── Private Helpers ────────────────────────────────────────────────────
    private static void AddReportTitle(IXLWorksheet ws, string title, int colSpan)
    {
        ws.Cell(1, 1).Value = title;
        var titleRange = ws.Range(1, 1, 1, colSpan);
        titleRange.Merge();
        titleRange.Style
            .Font.SetBold(true)
            .Font.SetFontSize(14)
            .Font.SetFontColor(XLColor.White)
            .Fill.SetBackgroundColor(HeaderBg)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
    }

    private static void AddHeaders(IXLWorksheet ws, int row, string[] headers)
    {
        for (int col = 1; col <= headers.Length; col++)
        {
            var cell = ws.Cell(row, col);
            cell.Value = headers[col - 1];
            cell.Style
                .Font.SetBold(true)
                .Font.SetFontColor(XLColor.White)
                .Fill.SetBackgroundColor(HeaderBg)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        }
    }

    private static void ApplyAlternateRowStyle(IXLWorksheet ws, int row, int colCount)
    {
        ws.Range(row, 1, row, colCount).Style.Fill.BackgroundColor = AlternateRow;
    }

    private static void AutoFitColumns(IXLWorksheet ws, int colCount)
    {
        for (int col = 1; col <= colCount; col++)
            ws.Column(col).AdjustToContents();
    }

    private static byte[] ToBytes(XLWorkbook wb)
    {
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }
}
