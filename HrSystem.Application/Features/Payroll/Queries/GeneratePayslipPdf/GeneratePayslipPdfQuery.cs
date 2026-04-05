using ErrorOr;
using HrSystem.Domain.Entities.Payroll;
using HrSystem.Infrustructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HrSystem.Application.Features.Payroll.Queries.GeneratePayslipPdf;

public record GeneratePayslipPdfQuery(Guid PayslipId) : IRequest<ErrorOr<byte[]>>;

public class GeneratePayslipPdfQueryHandler : IRequestHandler<GeneratePayslipPdfQuery, ErrorOr<byte[]>>
{
    private readonly ApplicationDbContext _context;

    public GeneratePayslipPdfQueryHandler(ApplicationDbContext context)
    {
        _context = context;
        
        // Configure QuestPDF License
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<ErrorOr<byte[]>> Handle(GeneratePayslipPdfQuery request, CancellationToken cancellationToken)
    {
        // Fetch payslip with all related data
        var payslip = await _context.Payslips
            .Include(p => p.Employee)
                .ThenInclude(e => e.Department)
            .Include(p => p.Employee)
                .ThenInclude(e => e.Branch)
            .Include(p => p.Employee)
                .ThenInclude(e => e.JobTitle)
            .Include(p => p.PayrollCycle)
            .Include(p => p.PayslipAllowances)
            .Include(p => p.PayslipDeductions)
            .FirstOrDefaultAsync(p => p.Id == request.PayslipId && !p.IsDeleted, cancellationToken);

        if (payslip == null)
            return Error.NotFound(description: "Payslip not found.");

        // Generate PDF
        var document = new PayslipDocument(payslip);
        var pdfBytes = document.GeneratePdf();

        return pdfBytes;
    }
}

public class PayslipDocument : IDocument
{
    private readonly Payslip _payslip;

    public PayslipDocument(Payslip payslip)
    {
        _payslip = payslip;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container
            .Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
    }

    private void ComposeHeader(IContainer container)
    {
        container.Background(Colors.Blue.Darken3).Padding(15).Column(column =>
        {
            column.Item().Text("PAYSLIP").FontSize(24).Bold().FontColor(Colors.White);
            column.Item().Text($"Period: {GetMonthName(_payslip.PayrollCycle.Month)} {_payslip.PayrollCycle.Year}")
                .FontSize(14).FontColor(Colors.White);
            column.Item().Text($"Payslip Number: {_payslip.PayslipNumber}")
                .FontSize(12).FontColor(Colors.Grey.Lighten2);
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingVertical(20).Column(column =>
        {
            // Employee Information Section
            column.Item().Element(ComposeEmployeeInfo);
            
            column.Item().PaddingTop(15).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

            // Salary Breakdown Section
            column.Item().PaddingTop(15).Element(ComposeSalaryBreakdown);

            column.Item().PaddingTop(15).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

            // Attendance Section
            column.Item().PaddingTop(15).Element(ComposeAttendance);

            column.Item().PaddingTop(15).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

            // Net Salary Section
            column.Item().PaddingTop(15).Element(ComposeNetSalary);
        });
    }

    private void ComposeEmployeeInfo(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().Text("EMPLOYEE INFORMATION").FontSize(14).Bold().FontColor(Colors.Blue.Darken3);
            
            column.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Row(r =>
                    {
                        r.AutoItem().Width(120).Text("Employee Code:").Bold();
                        r.RelativeItem().Text(_payslip.Employee.EmployeeCode);
                    });
                    col.Item().PaddingTop(5).Row(r =>
                    {
                        r.AutoItem().Width(120).Text("Employee Name:").Bold();
                        r.RelativeItem().Text(_payslip.Employee.FullNameEn);
                    });
                    col.Item().PaddingTop(5).Row(r =>
                    {
                        r.AutoItem().Width(120).Text("Department:").Bold();
                        r.RelativeItem().Text(_payslip.Employee.Department?.NameEn ?? "N/A");
                    });
                });

                row.RelativeItem().Column(col =>
                {
                    col.Item().Row(r =>
                    {
                        r.AutoItem().Width(120).Text("Branch:").Bold();
                        r.RelativeItem().Text(_payslip.Employee.Branch?.NameEn ?? "N/A");
                    });
                    col.Item().PaddingTop(5).Row(r =>
                    {
                        r.AutoItem().Width(120).Text("Position:").Bold();
                        r.RelativeItem().Text(_payslip.Employee.JobTitle?.TitleEn ?? "N/A");
                    });
                    col.Item().PaddingTop(5).Row(r =>
                    {
                        r.AutoItem().Width(120).Text("Generated Date:").Bold();
                        r.RelativeItem().Text(_payslip.GeneratedDate?.ToString("dd MMM yyyy") ?? "N/A");
                    });
                });
            });
        });
    }

    private void ComposeSalaryBreakdown(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().Text("SALARY BREAKDOWN").FontSize(14).Bold().FontColor(Colors.Blue.Darken3);

            // Earnings Section
            column.Item().PaddingTop(10).Background(Colors.Green.Lighten4).Padding(10).Column(earningsCol =>
            {
                earningsCol.Item().Text("EARNINGS").FontSize(12).Bold().FontColor(Colors.Green.Darken3);
                
                earningsCol.Item().PaddingTop(5).Row(row =>
                {
                    row.RelativeItem().Text("Basic Salary:");
                    row.AutoItem().Text($"{_payslip.BasicSalary:N2}").Bold();
                });

                // Allowances
                if (_payslip.PayslipAllowances != null && _payslip.PayslipAllowances.Count > 0)
                {
                    foreach (var allowance in _payslip.PayslipAllowances)
                    {
                        earningsCol.Item().PaddingTop(3).Row(row =>
                        {
                            row.RelativeItem().Text(allowance.AllowanceNameEn);
                            row.AutoItem().Text($"{allowance.Amount:N2}");
                        });
                    }
                }

                if (_payslip.OvertimeAmount > 0)
                {
                    earningsCol.Item().PaddingTop(3).Row(row =>
                    {
                        row.RelativeItem().Text("Overtime");
                        row.AutoItem().Text($"{_payslip.OvertimeAmount:N2}");
                    });
                }

                if (_payslip.BonusAmount > 0)
                {
                    earningsCol.Item().PaddingTop(3).Row(row =>
                    {
                        row.RelativeItem().Text("Bonus");
                        row.AutoItem().Text($"{_payslip.BonusAmount:N2}");
                    });
                }

                earningsCol.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Green.Darken2);
                earningsCol.Item().PaddingTop(5).Row(row =>
                {
                    row.RelativeItem().Text("Gross Salary:").Bold().FontSize(12);
                    row.AutoItem().Text($"{_payslip.GrossSalary:N2}").Bold().FontSize(12).FontColor(Colors.Green.Darken3);
                });
            });

            // Deductions Section
            column.Item().PaddingTop(10).Background(Colors.Red.Lighten4).Padding(10).Column(deductionsCol =>
            {
                deductionsCol.Item().Text("DEDUCTIONS").FontSize(12).Bold().FontColor(Colors.Red.Darken3);

                if (_payslip.IncomeTax > 0)
                {
                    deductionsCol.Item().PaddingTop(5).Row(row =>
                    {
                        row.RelativeItem().Text("Income Tax:");
                        row.AutoItem().Text($"{_payslip.IncomeTax:N2}");
                    });
                }

                if (_payslip.SocialInsuranceEmployee > 0)
                {
                    deductionsCol.Item().PaddingTop(3).Row(row =>
                    {
                        row.RelativeItem().Text("Social Insurance (Employee):");
                        row.AutoItem().Text($"{_payslip.SocialInsuranceEmployee:N2}");
                    });
                }

                if (_payslip.LeaveDeductions > 0)
                {
                    deductionsCol.Item().PaddingTop(3).Row(row =>
                    {
                        row.RelativeItem().Text("Leave Deductions:");
                        row.AutoItem().Text($"{_payslip.LeaveDeductions:N2}");
                    });
                }

                // Custom Deductions
                if (_payslip.PayslipDeductions != null && _payslip.PayslipDeductions.Count > 0)
                {
                    foreach (var deduction in _payslip.PayslipDeductions)
                    {
                        deductionsCol.Item().PaddingTop(3).Row(row =>
                        {
                            row.RelativeItem().Text(deduction.DeductionNameEn);
                            row.AutoItem().Text($"{deduction.Amount:N2}");
                        });
                    }
                }

                deductionsCol.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Red.Darken2);
                deductionsCol.Item().PaddingTop(5).Row(row =>
                {
                    row.RelativeItem().Text("Total Deductions:").Bold().FontSize(12);
                    row.AutoItem().Text($"{_payslip.TotalDeductions:N2}").Bold().FontSize(12).FontColor(Colors.Red.Darken3);
                });
            });

            // Employer Contribution (if any)
            if (_payslip.SocialInsuranceEmployer > 0)
            {
                column.Item().PaddingTop(10).Background(Colors.Blue.Lighten4).Padding(10).Row(row =>
                {
                    row.RelativeItem().Text("Employer Social Insurance Contribution:").Bold();
                    row.AutoItem().Text($"{_payslip.SocialInsuranceEmployer:N2}").Bold();
                });
            }
        });
    }

    private void ComposeAttendance(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().Text("ATTENDANCE SUMMARY").FontSize(14).Bold().FontColor(Colors.Blue.Darken3);

            column.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Row(r =>
                    {
                        r.AutoItem().Width(150).Text("Total Working Days:").Bold();
                        r.AutoItem().Text(_payslip.TotalWorkingDays.ToString());
                    });
                    col.Item().PaddingTop(5).Row(r =>
                    {
                        r.AutoItem().Width(150).Text("Actual Working Days:").Bold();
                        r.AutoItem().Text(_payslip.ActualWorkingDays.ToString());
                    });
                });

                row.RelativeItem().Column(col =>
                {
                    col.Item().Row(r =>
                    {
                        r.AutoItem().Width(150).Text("Absent Days:").Bold();
                        r.AutoItem().Text(_payslip.AbsentDays.ToString());
                    });
                    col.Item().PaddingTop(5).Row(r =>
                    {
                        r.AutoItem().Width(150).Text("Unpaid Leave Days:").Bold();
                        r.AutoItem().Text(_payslip.UnpaidLeaveDays.ToString());
                    });
                });
            });
        });
    }

    private void ComposeNetSalary(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().Background(Colors.Blue.Darken4).Padding(15).Row(row =>
            {
                row.RelativeItem().Text("NET SALARY").FontSize(16).Bold().FontColor(Colors.White);
                row.AutoItem().Text($"{_payslip.NetSalary:N2}").FontSize(20).Bold().FontColor(Colors.White);
            });

            if (_payslip.IsPaid && _payslip.PaidDate != null)
            {
                column.Item().PaddingTop(10).Background(Colors.Green.Lighten3).Padding(10).Row(row =>
                {
                    row.RelativeItem().Text("Payment Status: PAID").Bold().FontColor(Colors.Green.Darken4);
                    row.AutoItem().Text($"Paid on: {_payslip.PaidDate?.ToString("dd MMM yyyy")}").FontColor(Colors.Green.Darken4);
                });
            }
            else
            {
                column.Item().PaddingTop(10).Background(Colors.Orange.Lighten3).Padding(10).Text("Payment Status: PENDING")
                    .Bold().FontColor(Colors.Orange.Darken4);
            }
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(text =>
        {
            text.Span("Generated on: ").FontSize(9).FontColor(Colors.Grey.Medium);
            text.Span(DateTime.Now.ToString("dd MMM yyyy HH:mm")).FontSize(9).FontColor(Colors.Grey.Medium);
            text.Span(" | ").FontSize(9).FontColor(Colors.Grey.Medium);
            text.Span("This is a computer-generated document. No signature required.").FontSize(9).FontColor(Colors.Grey.Medium);
        });
    }

    private string GetMonthName(int month)
    {
        return new DateTime(2000, month, 1).ToString("MMMM");
    }
}
