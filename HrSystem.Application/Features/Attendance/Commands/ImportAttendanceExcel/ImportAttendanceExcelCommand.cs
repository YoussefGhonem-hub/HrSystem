using ErrorOr;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace HrSystem.Application.Features.Attendance.Commands.ImportAttendanceExcel;

/// <summary>
/// Import attendance records from an Excel file (fingerprint device export).
/// Expected columns: EmployeeCode, Date, CheckInTime, CheckOutTime (optional headers can vary).
/// </summary>
public record ImportAttendanceExcelCommand(
    Guid BranchId,
    IFormFile File,
    bool SkipDuplicates
) : IRequest<ErrorOr<GenericResponse<ImportAttendanceResultDto>>>;

public record ImportAttendanceResultDto
{
    public int TotalRows { get; init; }
    public int SuccessCount { get; init; }
    public int SkippedCount { get; init; }
    public int ErrorCount { get; init; }
    public List<ImportAttendanceErrorDto> Errors { get; init; } = new();
}

public record ImportAttendanceErrorDto
{
    public int RowNumber { get; init; }
    public string? EmployeeCode { get; init; }
    public string ErrorMessage { get; init; } = string.Empty;
}
