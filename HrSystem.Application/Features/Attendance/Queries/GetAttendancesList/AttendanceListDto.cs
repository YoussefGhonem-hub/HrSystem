namespace HrSystem.Application.Features.Attendance.Queries.GetAttendancesList;

public record AttendanceListDto
{
    public Guid Id { get; init; }
    public Guid EmployeeId { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public string EmployeeCode { get; init; } = string.Empty;
    public string? JobTitle { get; init; }
    public string? Department { get; init; }
    public DateTime Date { get; init; }
    public TimeSpan? CheckInTime { get; init; }
    public TimeSpan? CheckOutTime { get; init; }
    public Guid StatusId { get; init; }
    public string? StatusNameEn { get; init; }
    public string? StatusNameAr { get; init; }
    public TimeSpan? WorkedHours { get; init; }
    public string? HalfDayRule { get; init; }
    public bool IsLate { get; init; }
    public bool IsEarlyLeave { get; init; }
    public bool IsOvertime { get; init; }
}
