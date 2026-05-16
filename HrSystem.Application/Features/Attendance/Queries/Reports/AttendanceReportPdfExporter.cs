using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HrSystem.Application.Features.Attendance.Queries.Reports;

/// <summary>
/// Converts attendance report DTOs into PDF documents using QuestPDF.
/// Each public static method corresponds to one of the 7 report types.
/// </summary>
public static class AttendanceReportPdfExporter
{
    private const string HeaderColor = "#1F3864";
    private const string SubHeaderColor = "#D6E4F7";
    private const string AlternateRowColor = "F5F9FF";

    static AttendanceReportPdfExporter()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    // ── 1. Daily Attendance ──────────────────────────────────────────────────
    public static byte[] ExportDailyAttendance(DailyAttendanceReportDto report)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                ConfigurePage(page);
                page.Header().Element(c => RenderHeader(c, $"Daily Attendance Report", $"Date: {report.ReportDate:dd MMMM yyyy}"));
                page.Content().Element(c =>
                {
                    c.Column(col =>
                    {
                        col.Spacing(8);
                        col.Item().Element(inner => RenderSummaryBox(inner, new Dictionary<string, string>
                        {
                            ["Total Employees"] = report.TotalEmployees.ToString(),
                            ["Present"] = report.PresentCount.ToString(),
                            ["Absent"] = report.AbsentCount.ToString(),
                            ["Late"] = report.LateCount.ToString(),
                            ["Early Leave"] = report.EarlyLeaveCount.ToString(),
                            ["On Leave"] = report.OnLeaveCount.ToString(),
                            ["Attendance Rate"] = $"{report.AttendanceRate}%"
                        }));

                        col.Item().Element(inner => RenderTable(inner,
                            ["Code", "Name", "Dept", "Status", "Check-In", "Check-Out", "Late(m)", "Hrs"],
                            report.Rows.Select(r => new[]
                            {
                                r.EmployeeCode,
                                r.EmployeeName,
                                r.Department,
                                r.Status,
                                r.CheckIn?.ToString(@"hh\:mm") ?? "—",
                                r.CheckOut?.ToString(@"hh\:mm") ?? "—",
                                r.LateMinutes.ToString(),
                                r.WorkedHours.ToString("F1")
                            }).ToList()
                        ));
                    });
                });
                page.Footer().Element(RenderFooter);
            });
        }).GeneratePdf();
    }

    // ── 2. Monthly Summary ───────────────────────────────────────────────────
    public static byte[] ExportMonthlySummary(MonthlyAttendanceSummaryReportDto report)
    {
        var monthName = new DateTime(report.Year, report.Month, 1).ToString("MMMM yyyy");
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                ConfigurePage(page, PageSizes.A3.Landscape());
                page.Header().Element(c => RenderHeader(c, "Monthly Attendance Summary", monthName));
                page.Content().Element(c =>
                {
                    c.Column(col =>
                    {
                        col.Spacing(8);
                        col.Item().Element(inner => RenderSummaryBox(inner, new Dictionary<string, string>
                        {
                            ["Total Working Days"] = report.TotalWorkingDays.ToString()
                        }));

                        col.Item().Element(inner => RenderTable(inner,
                            ["Code", "Name", "Dept", "Present", "Absent", "Late", "Late(m)", "OT Hrs", "Leave", "Att%"],
                            report.Rows.Select(r => new[]
                            {
                                r.EmployeeCode,
                                r.EmployeeName,
                                r.Department,
                                r.DaysPresent.ToString(),
                                r.DaysAbsent.ToString(),
                                r.DaysLate.ToString(),
                                r.TotalLateMinutes.ToString("F0"),
                                r.OvertimeHours.ToString("F1"),
                                r.LeaveDaysTaken.ToString(),
                                $"{r.AttendancePercentage}%"
                            }).ToList()
                        ));
                    });
                });
                page.Footer().Element(RenderFooter);
            });
        }).GeneratePdf();
    }

    // ── 3. Payroll Attendance ────────────────────────────────────────────────
    public static byte[] ExportPayrollAttendance(PayrollAttendanceReportDto report)
    {
        var monthName = new DateTime(report.Year, report.Month, 1).ToString("MMMM yyyy");
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                ConfigurePage(page, PageSizes.A3.Landscape());
                page.Header().Element(c => RenderHeader(c, "Payroll Attendance Report", monthName));
                page.Content().Element(c =>
                {
                    c.Column(col =>
                    {
                        col.Spacing(8);
                        col.Item().Element(inner => RenderSummaryBox(inner, new Dictionary<string, string>
                        {
                            ["Working Days"] = report.TotalWorkingDays.ToString(),
                            ["Total Late Deductions"] = report.TotalLateDeductions.ToString("N2"),
                            ["Total OT Amounts"] = report.TotalOvertimeAmounts.ToString("N2")
                        }));

                        col.Item().Element(inner => RenderTable(inner,
                            ["Code", "Name", "Dept", "Basic", "Worked", "Absent", "Late(m)", "Abs.Ded.", "Late Ded.", "OT Hrs", "OT Amt", "Net Impact"],
                            report.Rows.Select(r => new[]
                            {
                                r.EmployeeCode,
                                r.EmployeeName,
                                r.Department,
                                r.BasicSalary.ToString("N2"),
                                r.WorkedDays.ToString(),
                                r.AbsentDays.ToString(),
                                r.LateMinutes.ToString("F0"),
                                r.AbsentDeductionAmount.ToString("N2"),
                                r.LateDeductionAmount.ToString("N2"),
                                r.OvertimeHours.ToString("F1"),
                                r.OvertimeAmount.ToString("N2"),
                                r.NetSalaryImpact.ToString("N2")
                            }).ToList()
                        ));
                    });
                });
                page.Footer().Element(RenderFooter);
            });
        }).GeneratePdf();
    }

    // ── 4. Late Arrivals ─────────────────────────────────────────────────────
    public static byte[] ExportLateArrivals(LateArrivalsReportDto report)
    {
        return Document.Create(container =>
        {
            // Detail page
            container.Page(page =>
            {
                ConfigurePage(page);
                page.Header().Element(c => RenderHeader(c, "Late Arrivals Report",
                    $"{report.Meta.FromDate:dd MMM yyyy} – {report.Meta.ToDate:dd MMM yyyy}"));
                page.Content().Element(c =>
                {
                    c.Column(col =>
                    {
                        col.Spacing(8);
                        col.Item().Element(inner => RenderSummaryBox(inner, new Dictionary<string, string>
                        {
                            ["Total Late Instances"] = report.TotalLateInstances.ToString()
                        }));

                        col.Item().Text("Detailed Records").Bold().FontSize(11);
                        col.Item().Element(inner => RenderTable(inner,
                            ["Code", "Name", "Dept", "Date", "Scheduled", "Actual", "Min Late"],
                            report.Rows.Select(r => new[]
                            {
                                r.EmployeeCode,
                                r.EmployeeName,
                                r.Department,
                                r.Date.ToString("dd/MM/yyyy"),
                                r.ScheduledStartTime?.ToString(@"hh\:mm") ?? "—",
                                r.ActualCheckIn?.ToString(@"hh\:mm") ?? "—",
                                r.MinutesLate.ToString()
                            }).ToList()
                        ));

                        col.Item().Text("Employee Summary").Bold().FontSize(11);
                        col.Item().Element(inner => RenderTable(inner,
                            ["Code", "Name", "Dept", "Frequency", "Total(min)", "Avg(min)"],
                            report.EmployeeSummary.Select(s => new[]
                            {
                                s.EmployeeCode,
                                s.EmployeeName,
                                s.Department,
                                s.LateFrequency.ToString(),
                                s.TotalLateMinutes.ToString("F0"),
                                s.AverageLateMinutes.ToString("F1")
                            }).ToList()
                        ));
                    });
                });
                page.Footer().Element(RenderFooter);
            });
        }).GeneratePdf();
    }

    // ── 5. Absenteeism ───────────────────────────────────────────────────────
    public static byte[] ExportAbsenteeism(AbsenteeismReportDto report)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                ConfigurePage(page);
                page.Header().Element(c => RenderHeader(c, "Absenteeism Report",
                    $"{report.Meta.FromDate:dd MMM yyyy} – {report.Meta.ToDate:dd MMM yyyy}"));
                page.Content().Element(c =>
                {
                    c.Column(col =>
                    {
                        col.Spacing(8);
                        col.Item().Element(inner => RenderSummaryBox(inner, new Dictionary<string, string>
                        {
                            ["Total Absent Days"] = report.TotalAbsentDays.ToString(),
                            ["Overall Absence Rate"] = $"{report.OverallAbsenceRate}%"
                        }));

                        col.Item().Text("Employee Absenteeism").Bold().FontSize(11);
                        col.Item().Element(inner => RenderTable(inner,
                            ["Code", "Name", "Dept", "Absent", "Authorized", "Unauthorized", "Rate%"],
                            report.Rows.Select(r => new[]
                            {
                                r.EmployeeCode,
                                r.EmployeeName,
                                r.Department,
                                r.AbsentDays.ToString(),
                                r.AuthorizedAbsenceDays.ToString(),
                                r.UnauthorizedAbsenceDays.ToString(),
                                $"{r.AbsenceRate}%"
                            }).ToList()
                        ));

                        col.Item().Text("Department Summary").Bold().FontSize(11);
                        col.Item().Element(inner => RenderTable(inner,
                            ["Department", "Employees", "Absent Days", "Rate%"],
                            report.DepartmentSummary.Select(d => new[]
                            {
                                d.DepartmentName,
                                d.TotalEmployees.ToString(),
                                d.TotalAbsentDays.ToString(),
                                $"{d.AbsenceRate}%"
                            }).ToList()
                        ));
                    });
                });
                page.Footer().Element(RenderFooter);
            });
        }).GeneratePdf();
    }

    // ── 6. Overtime ──────────────────────────────────────────────────────────
    public static byte[] ExportOvertime(OvertimeReportDto report)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                ConfigurePage(page);
                page.Header().Element(c => RenderHeader(c, "Overtime Report",
                    $"{report.Meta.FromDate:dd MMM yyyy} – {report.Meta.ToDate:dd MMM yyyy}"));
                page.Content().Element(c =>
                {
                    c.Column(col =>
                    {
                        col.Spacing(8);
                        col.Item().Element(inner => RenderSummaryBox(inner, new Dictionary<string, string>
                        {
                            ["Total OT Hours"] = report.TotalOvertimeHours.ToString("F1"),
                            ["Total OT Cost"] = report.TotalOvertimeCost.ToString("N2")
                        }));

                        col.Item().Text("Employee Overtime Detail").Bold().FontSize(11);
                        col.Item().Element(inner => RenderTable(inner,
                            ["Code", "Name", "Dept", "Regular Hrs", "OT Hrs", "Multiplier", "Hourly Rate", "OT Cost"],
                            report.Rows.Select(r => new[]
                            {
                                r.EmployeeCode,
                                r.EmployeeName,
                                r.Department,
                                r.RegularHours.ToString("F1"),
                                r.OvertimeHours.ToString("F1"),
                                r.OvertimeMultiplier.ToString("F1"),
                                r.HourlyRate.ToString("N2"),
                                r.OvertimeCost.ToString("N2")
                            }).ToList()
                        ));

                        col.Item().Text("Department Summary").Bold().FontSize(11);
                        col.Item().Element(inner => RenderTable(inner,
                            ["Department", "Total OT Hours", "Total OT Cost"],
                            report.DepartmentSummary.Select(d => new[]
                            {
                                d.DepartmentName,
                                d.TotalOvertimeHours.ToString("F1"),
                                d.TotalOvertimeCost.ToString("N2")
                            }).ToList()
                        ));
                    });
                });
                page.Footer().Element(RenderFooter);
            });
        }).GeneratePdf();
    }

    // ── 7. Department Attendance ─────────────────────────────────────────────
    public static byte[] ExportDepartmentAttendance(DepartmentAttendanceReportDto report)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                ConfigurePage(page);
                page.Header().Element(c => RenderHeader(c, "Department Attendance Report",
                    $"{report.Meta.FromDate:dd MMM yyyy} – {report.Meta.ToDate:dd MMM yyyy}"));
                page.Content().Element(c =>
                {
                    c.Column(col =>
                    {
                        col.Spacing(8);
                        col.Item().Element(inner => RenderSummaryBox(inner, new Dictionary<string, string>
                        {
                            ["Total Working Days"] = report.TotalWorkingDays.ToString()
                        }));

                        col.Item().Element(inner => RenderTable(inner,
                            ["Department", "Employees", "Avg Att%", "Absent Days", "Late Instances", "OT Hours"],
                            report.Rows.Select(r => new[]
                            {
                                r.DepartmentName,
                                r.TotalEmployees.ToString(),
                                $"{r.AverageAttendancePercentage}%",
                                r.TotalAbsentDays.ToString(),
                                r.TotalLateInstances.ToString(),
                                r.TotalOvertimeHours.ToString("F1")
                            }).ToList()
                        ));
                    });
                });
                page.Footer().Element(RenderFooter);
            });
        }).GeneratePdf();
    }

    // ─── Private Layout Helpers ──────────────────────────────────────────────
    private static void ConfigurePage(PageDescriptor page, PageSize? size = null)
    {
        page.Size(size ?? PageSizes.A4.Landscape());
        page.Margin(30);
        page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));
    }

    private static void RenderHeader(IContainer container, string title, string subtitle)
    {
        container.Background(HeaderColor).Padding(12).Column(col =>
        {
            col.Item().Text(title)
                .Bold().FontSize(16).FontColor(Colors.White);
            col.Item().Text(subtitle)
                .FontSize(10).FontColor(Colors.White);
            col.Item().Text($"Generated: {DateTime.UtcNow:dd MMM yyyy HH:mm} UTC")
                .FontSize(8).FontColor(Colors.Grey.Lighten3);
        });
    }

    private static void RenderSummaryBox(IContainer container, Dictionary<string, string> values)
    {
        container.Background(SubHeaderColor).Padding(8).Row(row =>
        {
            foreach (var kv in values)
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(kv.Key).FontSize(8).FontColor(Colors.Grey.Darken2);
                    col.Item().Text(kv.Value).Bold().FontSize(11);
                });
            }
        });
    }

    private static void RenderTable(IContainer container, string[] headers, List<string[]> rows)
    {
        container.Table(table =>
        {
            // Column definitions
            table.ColumnsDefinition(cols =>
            {
                for (int i = 0; i < headers.Length; i++)
                    cols.RelativeColumn();
            });

            // Header row
            foreach (var h in headers)
            {
                table.Header(header =>
                {
                    header.Cell().Background(HeaderColor).Padding(4)
                        .Text(h).Bold().FontSize(9).FontColor(Colors.White);
                });
            }

            // Data rows
            for (int i = 0; i < rows.Count; i++)
            {
                var bgColor = i % 2 == 0 ? "FFFFFF" : AlternateRowColor;
                foreach (var cell in rows[i])
                {
                    table.Cell().Background(bgColor).Padding(3).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
                        .Text(cell ?? "—").FontSize(8);
                }
            }
        });
    }

    private static void RenderFooter(IContainer container)
    {
        container.BorderTop(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Row(row =>
        {
            row.RelativeItem().Text("HR System — Attendance Report").FontSize(8).FontColor(Colors.Grey.Darken1);
            row.RelativeItem().AlignRight().Text(text =>
            {
                text.Span("Page ").FontSize(8);
                text.CurrentPageNumber().FontSize(8);
                text.Span(" of ").FontSize(8);
                text.TotalPages().FontSize(8);
            });
        });
    }
}
