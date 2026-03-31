using ClosedXML.Excel;
using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Payroll.Queries.ExportBankFile;

public record ExportBankFileQuery(
    int Month,
    int Year,
    Guid? ProfileId
) : IRequest<ErrorOr<GenericResponse<BankFileExportDto>>>;

public class BankFileExportDto
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    public byte[] FileContent { get; set; } = Array.Empty<byte>();
    public int TotalRecords { get; set; }
    public decimal TotalAmount { get; set; }
    public string CycleName { get; set; } = string.Empty;
}

public class ExportBankFileQueryHandler
    : IRequestHandler<ExportBankFileQuery, ErrorOr<GenericResponse<BankFileExportDto>>>
{
    private readonly ApplicationDbContext _context;

    public ExportBankFileQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<BankFileExportDto>>> Handle(
        ExportBankFileQuery request,
        CancellationToken cancellationToken)
    {
        if (request.Month < 1 || request.Month > 12)
            return Error.Validation(description: "Month must be between 1 and 12.");
        if (request.Year < 2000 || request.Year > 2100)
            return Error.Validation(description: "Invalid year.");

        // Get bank export profile
        Domain.Entities.Payroll.BankExportProfile? profile = null;
        if (request.ProfileId.HasValue)
        {
            profile = await _context.BankExportProfiles
                .FirstOrDefaultAsync(b => b.Id == request.ProfileId.Value && !b.IsDeleted, cancellationToken);
        }
        else
        {
            profile = await _context.BankExportProfiles
                .FirstOrDefaultAsync(b => !b.IsDeleted && b.IsDefault, cancellationToken);
        }

        if (profile == null)
            return Error.NotFound(description: "No bank export profile found. Please create a bank export profile first.");

        var isSuperOrOrgAdmin = CurrentUser.Roles?.Contains(RoleNames.SuperAdmin) == true
            || CurrentUser.Roles?.Contains(RoleNames.OrganizationAdmin) == true;
        var branchId = isSuperOrOrgAdmin ? (Guid?)null : CurrentUser.BranchId;

        var cycle = await _context.PayrollCycles
            .FirstOrDefaultAsync(c => c.Month == request.Month && c.Year == request.Year, cancellationToken);

        if (cycle == null)
            return Error.NotFound(description: $"No payroll cycle found for {request.Month}/{request.Year}.");

        var query = _context.Payslips
            .Include(p => p.Employee)
                .ThenInclude(e => e.Salaries)
            .Where(p => !p.IsDeleted
                && p.PayrollCycleId == cycle.Id
                && !p.IsPaid);

        if (branchId.HasValue)
            query = query.Where(p => p.Employee.BranchId == branchId.Value);

        var payslips = await query.ToListAsync(cancellationToken);

        if (payslips.Count == 0)
            return Error.NotFound(description: "No unpaid payslips found for this period.");

        // Build employee rows
        var employeeRows = new List<EmployeePaymentRow>();
        foreach (var p in payslips)
        {
            var salary = p.Employee.Salaries
                .Where(s => s.IsCurrent && !s.IsDeleted)
                .OrderByDescending(s => s.EffectiveDate)
                .FirstOrDefault();

            employeeRows.Add(new EmployeePaymentRow
            {
                BicCode = salary?.BankSwiftCode ?? profile.BicCode ?? "",
                AccountNumber = salary?.BankAccountNumber ?? "",
                AccountName = p.Employee.FullNameEn ?? p.Employee.FullNameAr ?? "",
                CreditAmount = p.NetSalary
            });
        }

        var totalAmount = employeeRows.Sum(r => r.CreditAmount);
        var fileDate = DateTime.UtcNow.ToString("dd/MM/yyyy");
        var fileContent = GenerateCibExcel(profile, employeeRows, fileDate, totalAmount);
        var fileName = $"CIB_Payroll_{cycle.CycleName.Replace(" ", "_")}_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";

        var dto = new BankFileExportDto
        {
            FileName = fileName,
            FileContent = fileContent,
            TotalRecords = employeeRows.Count,
            TotalAmount = totalAmount,
            CycleName = cycle.CycleName
        };

        return GenericResponse<BankFileExportDto>.SuccessResult(dto, "Bank file generated successfully.");
    }

    private static byte[] GenerateCibExcel(
        Domain.Entities.Payroll.BankExportProfile profile,
        List<EmployeePaymentRow> employees,
        string fileDate,
        decimal totalAmount)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Payroll");

        // === Row 1: Headers ===
        var headers = new[] { "File_Date", "Value_Date", "Narrative", "Currency", "Creditor_BIC_Code", "Account_Number", "Account_Name", "Debit_Amount", "Credit_Amount" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#4472C4");
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        // === Row 2: Company Debit Row ===
        ws.Cell(2, 1).Value = fileDate;                          // File_Date
        ws.Cell(2, 2).Value = fileDate;                          // Value_Date
        ws.Cell(2, 3).Value = profile.Narrative;                 // Narrative
        ws.Cell(2, 4).Value = profile.Currency.ToLower();        // Currency
        // Column 5 (Creditor_BIC_Code) empty for debit row
        ws.Cell(2, 6).Value = profile.CompanyAccountNumber;      // Account_Number
        ws.Cell(2, 6).Style.NumberFormat.Format = "@";
        ws.Cell(2, 7).Value = profile.CompanyAccountName;        // Account_Name
        ws.Cell(2, 8).Value = totalAmount;                       // Debit_Amount = sum of credits
        ws.Cell(2, 8).Style.NumberFormat.Format = "#,##0.00";
        // Column 9 (Credit_Amount) empty for debit row

        // Style company row
        ws.Range(2, 1, 2, 9).Style.Fill.BackgroundColor = XLColor.FromHtml("#D9E2F3");
        ws.Range(2, 1, 2, 9).Style.Font.Bold = true;

        // === Rows 3+: Employee Credit Rows ===
        for (int i = 0; i < employees.Count; i++)
        {
            int row = i + 3;
            var emp = employees[i];
            // Columns 1-4 empty for credit rows
            ws.Cell(row, 5).Value = emp.BicCode;                 // Creditor_BIC_Code
            ws.Cell(row, 6).Value = emp.AccountNumber;           // Account_Number
            ws.Cell(row, 6).Style.NumberFormat.Format = "@";
            ws.Cell(row, 7).Value = emp.AccountName;             // Account_Name
            // Column 8 (Debit_Amount) empty for credit rows
            ws.Cell(row, 9).Value = emp.CreditAmount;            // Credit_Amount
            ws.Cell(row, 9).Style.NumberFormat.Format = "#,##0.00";

            // Alternating row colors
            if (i % 2 == 1)
                ws.Range(row, 1, row, 9).Style.Fill.BackgroundColor = XLColor.FromHtml("#F2F2F2");
        }

        // === Summary Row ===
        int summaryRow = employees.Count + 3;
        ws.Cell(summaryRow, 7).Value = employees.Count;          // Count
        ws.Cell(summaryRow, 7).Style.Font.Bold = true;
        ws.Cell(summaryRow, 8).Value = totalAmount;              // Total Debit
        ws.Cell(summaryRow, 8).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(summaryRow, 8).Style.Font.Bold = true;
        ws.Cell(summaryRow, 9).FormulaA1 = $"SUM(I3:I{employees.Count + 2})";  // Total Credit
        ws.Cell(summaryRow, 9).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(summaryRow, 9).Style.Font.Bold = true;
        ws.Range(summaryRow, 1, summaryRow, 9).Style.Border.TopBorder = XLBorderStyleValues.Thin;

        // Column widths
        ws.Column(1).Width = 14;
        ws.Column(2).Width = 14;
        ws.Column(3).Width = 14;
        ws.Column(4).Width = 10;
        ws.Column(5).Width = 18;
        ws.Column(6).Width = 20;
        ws.Column(7).Width = 25;
        ws.Column(8).Width = 16;
        ws.Column(9).Width = 16;

        // Add borders to all data
        var dataRange = ws.Range(1, 1, summaryRow, 9);
        dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private class EmployeePaymentRow
    {
        public string BicCode { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public decimal CreditAmount { get; set; }
    }
}
