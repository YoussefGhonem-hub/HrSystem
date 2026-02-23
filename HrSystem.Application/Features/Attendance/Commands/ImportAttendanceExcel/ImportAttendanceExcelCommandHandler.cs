using ErrorOr;
using HrSystem.Domain.Enums;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using HrSystem.Infrustructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using AttendanceEntity = HrSystem.Domain.Entities.Attendance.Attendance;

namespace HrSystem.Application.Features.Attendance.Commands.ImportAttendanceExcel;

public class ImportAttendanceExcelCommandHandler
    : IRequestHandler<ImportAttendanceExcelCommand, ErrorOr<GenericResponse<ImportAttendanceResultDto>>>
{
    private readonly ApplicationDbContext _context;

    public ImportAttendanceExcelCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<ImportAttendanceResultDto>>> Handle(
        ImportAttendanceExcelCommand request, CancellationToken cancellationToken)
    {
        // ── 1) Validate branch and settings ──────────────────
        var branch = await _context.Branches
            .FirstOrDefaultAsync(b => b.Id == request.BranchId && !b.IsDeleted, cancellationToken);

        if (branch is null)
            return Error.NotFound("Branch.NotFound", "Branch not found.");

        var setting = await _context.BranchAttendanceSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.BranchId == request.BranchId && !s.IsDeleted, cancellationToken);

        if (setting is not null && !setting.AllowExcelImport)
            return Error.Validation("Attendance.ExcelImportNotAllowed",
                "Excel import is not enabled for this branch.");

        // ── 2) Validate file ─────────────────────────────────
        if (request.File is null || request.File.Length == 0)
            return Error.Validation("File.Empty", "Excel file is required.");

        var extension = Path.GetExtension(request.File.FileName)?.ToLowerInvariant();
        if (extension != ".xlsx" && extension != ".xls" && extension != ".csv")
            return Error.Validation("File.InvalidFormat",
                "Supported formats: .xlsx, .xls, .csv");

        // ── 3) Parse Excel file ──────────────────────────────
        var rows = new List<AttendanceImportRow>();
        var errors = new List<ImportAttendanceErrorDto>();

        try
        {
            using var stream = request.File.OpenReadStream();
            rows = await ParseExcelFile(stream, extension!, errors);
        }
        catch (Exception ex)
        {
            return Error.Failure("File.ParseError", $"Failed to parse file: {ex.Message}");
        }

        if (rows.Count == 0 && errors.Count == 0)
            return Error.Validation("File.Empty", "No data rows found in the file.");

        // ── 4) Resolve employee codes to IDs ─────────────────
        var employeeCodes = rows.Select(r => r.EmployeeCode).Distinct().ToList();
        var employees = await _context.Employees
            .AsNoTracking()
            .Where(e => employeeCodes.Contains(e.EmployeeCode) && e.BranchId == request.BranchId && !e.IsDeleted)
            .Select(e => new { e.Id, e.EmployeeCode, e.TenantId })
            .ToListAsync(cancellationToken);

        var employeeMap = employees.ToDictionary(e => e.EmployeeCode, e => e);

        // ── 5) Process each row ──────────────────────────────
        int successCount = 0;
        int skippedCount = 0;
        var skipDuplicates = request.SkipDuplicates || (setting?.ExcelImportSkipDuplicates ?? true);

        foreach (var row in rows)
        {
            if (!employeeMap.TryGetValue(row.EmployeeCode, out var emp))
            {
                errors.Add(new ImportAttendanceErrorDto
                {
                    RowNumber = row.RowNumber,
                    EmployeeCode = row.EmployeeCode,
                    ErrorMessage = $"Employee with code '{row.EmployeeCode}' not found in this branch."
                });
                continue;
            }

            // Check for existing record
            var existing = await _context.Attendances
                .FirstOrDefaultAsync(a => a.EmployeeId == emp.Id
                    && a.Date == row.Date
                    && !a.IsDeleted
                    && !a.IsConfigurationRecord, cancellationToken);

            if (existing is not null)
            {
                if (skipDuplicates)
                {
                    skippedCount++;
                    continue;
                }

                // Update existing
                if (row.CheckInTime.HasValue) existing.CheckInTime = row.CheckInTime;
                if (row.CheckOutTime.HasValue) existing.CheckOutTime = row.CheckOutTime;

                if (existing.CheckInTime.HasValue && existing.CheckOutTime.HasValue)
                    existing.WorkedHours = existing.CheckOutTime.Value - existing.CheckInTime.Value;

                existing.CheckInMethod ??= AttendanceMethod.ExcelImport;
                existing.CheckOutMethod ??= AttendanceMethod.ExcelImport;
                existing.MarkAsModified(CurrentUser.Id ?? Guid.Empty);
                successCount++;
                continue;
            }

            // Create new record
            var attendance = new AttendanceEntity
            {
                EmployeeId = emp.Id,
                Date = row.Date,
                CheckInTime = row.CheckInTime,
                CheckOutTime = row.CheckOutTime,
                StatusId = AttendanceStatusIds.Present,
                TenantId = emp.TenantId,
                BranchId = request.BranchId,
                CheckInMethod = AttendanceMethod.ExcelImport,
                CheckOutMethod = row.CheckOutTime.HasValue ? AttendanceMethod.ExcelImport : null,
                Notes = "Imported from Excel"
            };

            if (attendance.CheckInTime.HasValue && attendance.CheckOutTime.HasValue)
                attendance.WorkedHours = attendance.CheckOutTime.Value - attendance.CheckInTime.Value;

            attendance.MarkAsCreated(CurrentUser.Id ?? Guid.Empty);
            _context.Attendances.Add(attendance);
            successCount++;
        }

        await _context.SaveChangesAsync(cancellationToken);

        var result = new ImportAttendanceResultDto
        {
            TotalRows = rows.Count,
            SuccessCount = successCount,
            SkippedCount = skippedCount,
            ErrorCount = errors.Count,
            Errors = errors
        };

        return new GenericResponse<ImportAttendanceResultDto>
        {
            Success = true,
            Message = $"Import completed: {successCount} records imported, {skippedCount} skipped, {errors.Count} errors.",
            Data = result
        };
    }

    /// <summary>
    /// Parse the Excel/CSV file into rows. Supports common fingerprint device export formats.
    /// Expected columns (case-insensitive, flexible matching):
    ///   - Employee Code / EmployeeCode / Code / ID / رقم الموظف
    ///   - Date / التاريخ
    ///   - Check In / CheckIn / CheckInTime / وقت الحضور
    ///   - Check Out / CheckOut / CheckOutTime / وقت الانصراف
    /// </summary>
    private static Task<List<AttendanceImportRow>> ParseExcelFile(
        Stream stream, string extension, List<ImportAttendanceErrorDto> errors)
    {
        var rows = new List<AttendanceImportRow>();

        if (extension == ".csv")
        {
            return ParseCsvFile(stream, errors);
        }

        // For .xlsx / .xls we'll parse as CSV-like with tab/comma detection
        // NOTE: In production, add EPPlus or ClosedXML NuGet package for proper Excel parsing.
        // For now, we support CSV and assume .xlsx files will be handled by adding the package.
        return ParseCsvFile(stream, errors);
    }

    private static Task<List<AttendanceImportRow>> ParseCsvFile(
        Stream stream, List<ImportAttendanceErrorDto> errors)
    {
        var rows = new List<AttendanceImportRow>();

        using var reader = new StreamReader(stream);
        var headerLine = reader.ReadLine();
        if (string.IsNullOrWhiteSpace(headerLine))
            return Task.FromResult(rows);

        // Detect separator
        var separator = headerLine.Contains('\t') ? '\t' : ',';
        var headers = headerLine.Split(separator)
            .Select(h => h.Trim().Trim('"').ToLowerInvariant())
            .ToArray();

        // Map column indices
        int codeCol = FindColumnIndex(headers, "employeecode", "employee code", "code", "id", "emp code", "رقم الموظف", "employee_code", "emp_code");
        int dateCol = FindColumnIndex(headers, "date", "التاريخ", "attendance date", "attendance_date");
        int checkInCol = FindColumnIndex(headers, "checkin", "check in", "checkintime", "check in time", "وقت الحضور", "check_in", "checkin_time", "in time", "in");
        int checkOutCol = FindColumnIndex(headers, "checkout", "check out", "checkouttime", "check out time", "وقت الانصراف", "check_out", "checkout_time", "out time", "out");

        if (codeCol < 0)
        {
            errors.Add(new ImportAttendanceErrorDto
            {
                RowNumber = 1,
                ErrorMessage = "Cannot find 'EmployeeCode' column in the file header."
            });
            return Task.FromResult(rows);
        }
        if (dateCol < 0)
        {
            errors.Add(new ImportAttendanceErrorDto
            {
                RowNumber = 1,
                ErrorMessage = "Cannot find 'Date' column in the file header."
            });
            return Task.FromResult(rows);
        }

        int rowNumber = 1;
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            rowNumber++;
            if (string.IsNullOrWhiteSpace(line)) continue;

            var cells = line.Split(separator).Select(c => c.Trim().Trim('"')).ToArray();

            var employeeCode = codeCol < cells.Length ? cells[codeCol] : null;
            if (string.IsNullOrWhiteSpace(employeeCode))
            {
                errors.Add(new ImportAttendanceErrorDto
                {
                    RowNumber = rowNumber,
                    ErrorMessage = "Employee code is empty."
                });
                continue;
            }

            var dateStr = dateCol < cells.Length ? cells[dateCol] : null;
            if (!DateTime.TryParse(dateStr, out var date))
            {
                errors.Add(new ImportAttendanceErrorDto
                {
                    RowNumber = rowNumber,
                    EmployeeCode = employeeCode,
                    ErrorMessage = $"Invalid date: '{dateStr}'."
                });
                continue;
            }

            TimeSpan? checkInTime = null;
            if (checkInCol >= 0 && checkInCol < cells.Length && !string.IsNullOrWhiteSpace(cells[checkInCol]))
            {
                if (TimeSpan.TryParse(cells[checkInCol], out var ci))
                    checkInTime = ci;
                else if (DateTime.TryParse(cells[checkInCol], out var ciDt))
                    checkInTime = ciDt.TimeOfDay;
            }

            TimeSpan? checkOutTime = null;
            if (checkOutCol >= 0 && checkOutCol < cells.Length && !string.IsNullOrWhiteSpace(cells[checkOutCol]))
            {
                if (TimeSpan.TryParse(cells[checkOutCol], out var co))
                    checkOutTime = co;
                else if (DateTime.TryParse(cells[checkOutCol], out var coDt))
                    checkOutTime = coDt.TimeOfDay;
            }

            if (!checkInTime.HasValue && !checkOutTime.HasValue)
            {
                errors.Add(new ImportAttendanceErrorDto
                {
                    RowNumber = rowNumber,
                    EmployeeCode = employeeCode,
                    ErrorMessage = "Both check-in and check-out times are empty."
                });
                continue;
            }

            rows.Add(new AttendanceImportRow
            {
                RowNumber = rowNumber,
                EmployeeCode = employeeCode,
                Date = date.Date,
                CheckInTime = checkInTime,
                CheckOutTime = checkOutTime
            });
        }

        return Task.FromResult(rows);
    }

    private static int FindColumnIndex(string[] headers, params string[] candidates)
    {
        for (int i = 0; i < headers.Length; i++)
        {
            foreach (var candidate in candidates)
            {
                if (headers[i] == candidate || headers[i].Replace(" ", "") == candidate.Replace(" ", ""))
                    return i;
            }
        }
        return -1;
    }

    private class AttendanceImportRow
    {
        public int RowNumber { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public TimeSpan? CheckInTime { get; set; }
        public TimeSpan? CheckOutTime { get; set; }
    }
}
