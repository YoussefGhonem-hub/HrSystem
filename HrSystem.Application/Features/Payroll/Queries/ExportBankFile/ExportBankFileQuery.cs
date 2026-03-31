using ClosedXML.Excel;
using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Storage.AWS3.Services;

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
    private readonly IStorageService _storageService;

    public ExportBankFileQueryHandler(ApplicationDbContext context, IStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
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
        var fileDate = new DateTime(request.Year, request.Month, DateTime.DaysInMonth(request.Year, request.Month)).ToString("dd/MM/yyyy");

        byte[] fileContent;
        string fileName;

        // If profile has a template, fill data into template; otherwise generate from scratch
        if (!string.IsNullOrEmpty(profile.TemplateFileKey))
        {
            fileContent = await FillTemplateExcel(profile, employeeRows, fileDate, totalAmount, cancellationToken);
            var templateExt = Path.GetExtension(profile.TemplateFileName ?? ".xlsx");
            fileName = $"CIB_Payroll_{cycle.CycleName.Replace(" ", "_")}_{DateTime.UtcNow:yyyyMMddHHmmss}{templateExt}";
        }
        else
        {
            fileContent = GenerateCibExcel(profile, employeeRows, fileDate, totalAmount);
            fileName = $"CIB_Payroll_{cycle.CycleName.Replace(" ", "_")}_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
        }

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

    /// <summary>
    /// Download the bank template from S3 and fill employee payment data into it.
    /// Template format (CIB-style):
    ///   Row 1: Headers
    ///   Row 2: Company debit row (dates, narrative, currency, account, debit amount)
    ///   Rows 3..N: Employee credit rows (BIC, account, name, credit amount)
    ///   Last used row: Summary with COUNTA/SUM formulas
    /// </summary>
    private async Task<byte[]> FillTemplateExcel(
        Domain.Entities.Payroll.BankExportProfile profile,
        List<EmployeePaymentRow> employees,
        string fileDate,
        decimal totalAmount,
        CancellationToken cancellationToken)
    {
        var downloaded = await _storageService.DownloadFile(profile.TemplateFileKey!, cancellationToken);
        using var templateStream = new MemoryStream(downloaded.Contents);
        using var workbook = new XLWorkbook(templateStream);

        var ws = workbook.Worksheets.First();

        // --- Detect template structure ---
        // Row 1 = headers, Row 2 = company row
        // Find last row with data to detect summary row
        var lastRowUsed = ws.LastRowUsed()?.RowNumber() ?? 2;

        // Row 2: Fill company debit info
        ws.Cell(2, 1).Value = fileDate;                          // File_Date (A2)
        ws.Cell(2, 2).Value = fileDate;                          // Value_Date (B2)
        ws.Cell(2, 3).Value = profile.Narrative;                 // Narrative (C2)
        ws.Cell(2, 4).Value = profile.Currency.ToLower();        // Currency (D2)
        // E2: Creditor_BIC_Code - leave empty for company row
        ws.Cell(2, 6).Value = profile.CompanyAccountNumber;      // Account_Number (F2)
        ws.Cell(2, 7).Value = profile.CompanyAccountName;        // Account_Name (G2)
        ws.Cell(2, 8).Value = totalAmount;                       // Debit_Amount (H2)
        // I2: Credit_Amount - leave empty for debit row

        // --- Fill employee rows starting from Row 3 ---
        // First, clear any existing employee data rows (between row 3 and summary row)
        int summaryRow = lastRowUsed; // The last row is the summary row
        int templateEmployeeRows = summaryRow - 3; // Number of placeholder rows in template

        // Clear old employee data (rows 3 to summaryRow-1)
        for (int r = 3; r < summaryRow; r++)
        {
            // Only clear data cells, preserve formatting
            ws.Cell(r, 1).Value = "";   // File_Date
            ws.Cell(r, 2).Value = "";   // Value_Date
            ws.Cell(r, 3).Value = "";   // Narrative
            ws.Cell(r, 4).Value = "";   // Currency
            // Don't clear BIC if template has it pre-filled - we'll overwrite only if we have data
            ws.Cell(r, 6).Value = "";   // Account_Number
            ws.Cell(r, 7).Value = "";   // Account_Name
            ws.Cell(r, 8).Value = "";   // Debit_Amount
            ws.Cell(r, 9).Value = "";   // Credit_Amount
        }

        // If we need more rows than the template has, insert them
        if (employees.Count > templateEmployeeRows)
        {
            int rowsToInsert = employees.Count - templateEmployeeRows;
            // Insert before summary row to push it down
            ws.Row(summaryRow).InsertRowsAbove(rowsToInsert);
            summaryRow += rowsToInsert;
        }
        // If we have fewer employees than template rows, delete excess
        else if (employees.Count < templateEmployeeRows)
        {
            int rowsToDelete = templateEmployeeRows - employees.Count;
            int deleteStart = 3 + employees.Count;
            ws.Rows(deleteStart, deleteStart + rowsToDelete - 1).Delete();
            summaryRow -= rowsToDelete;
        }

        // Fill employee data
        for (int i = 0; i < employees.Count; i++)
        {
            int row = i + 3;
            var emp = employees[i];

            // Columns A-D empty for credit rows
            ws.Cell(row, 5).Value = emp.BicCode;                 // Creditor_BIC_Code (E)
            ws.Cell(row, 6).Value = emp.AccountNumber;           // Account_Number (F)
            ws.Cell(row, 7).Value = emp.AccountName;             // Account_Name (G)
            // Column H (Debit_Amount) empty for credit rows
            ws.Cell(row, 9).Value = emp.CreditAmount;            // Credit_Amount (I)
        }

        // Update summary row formulas
        int firstEmpRow = 3;
        int lastEmpRow = 2 + employees.Count;

        // Column G: Employee count (COUNTA)
        ws.Cell(summaryRow, 7).FormulaA1 = $"COUNTA(G{firstEmpRow}:G{lastEmpRow})";
        // Column H: Total debit (same as row 2 debit)
        ws.Cell(summaryRow, 8).Value = totalAmount;
        // Column I: Sum of credits
        ws.Cell(summaryRow, 9).FormulaA1 = $"SUM(I{firstEmpRow}:I{lastEmpRow})";

        using var outputStream = new MemoryStream();
        workbook.SaveAs(outputStream);
        return outputStream.ToArray();
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
        ws.Cell(2, 1).Value = fileDate;
        ws.Cell(2, 2).Value = fileDate;
        ws.Cell(2, 3).Value = profile.Narrative;
        ws.Cell(2, 4).Value = profile.Currency.ToLower();
        ws.Cell(2, 6).Value = profile.CompanyAccountNumber;
        ws.Cell(2, 6).Style.NumberFormat.Format = "@";
        ws.Cell(2, 7).Value = profile.CompanyAccountName;
        ws.Cell(2, 8).Value = totalAmount;
        ws.Cell(2, 8).Style.NumberFormat.Format = "#,##0.00";

        ws.Range(2, 1, 2, 9).Style.Fill.BackgroundColor = XLColor.FromHtml("#D9E2F3");
        ws.Range(2, 1, 2, 9).Style.Font.Bold = true;

        // === Rows 3+: Employee Credit Rows ===
        for (int i = 0; i < employees.Count; i++)
        {
            int row = i + 3;
            var emp = employees[i];
            ws.Cell(row, 5).Value = emp.BicCode;
            ws.Cell(row, 6).Value = emp.AccountNumber;
            ws.Cell(row, 6).Style.NumberFormat.Format = "@";
            ws.Cell(row, 7).Value = emp.AccountName;
            ws.Cell(row, 9).Value = emp.CreditAmount;
            ws.Cell(row, 9).Style.NumberFormat.Format = "#,##0.00";

            if (i % 2 == 1)
                ws.Range(row, 1, row, 9).Style.Fill.BackgroundColor = XLColor.FromHtml("#F2F2F2");
        }

        // === Summary Row ===
        int summaryRow = employees.Count + 3;
        ws.Cell(summaryRow, 7).Value = employees.Count;
        ws.Cell(summaryRow, 7).Style.Font.Bold = true;
        ws.Cell(summaryRow, 8).Value = totalAmount;
        ws.Cell(summaryRow, 8).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(summaryRow, 8).Style.Font.Bold = true;
        ws.Cell(summaryRow, 9).FormulaA1 = $"SUM(I3:I{employees.Count + 2})";
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
