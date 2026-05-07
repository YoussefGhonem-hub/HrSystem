namespace HrSystem.Application.Features.Attendance.Queries.GetAttendanceById;

public record AttendanceDto
{
    public Guid Id { get; init; }
    public Guid EmployeeId { get; init; }
    public string? EmployeeName { get; init; }
    public string? EmployeeCode { get; init; }
    public DateTime Date { get; init; }
    public TimeSpan? CheckInTime { get; init; }
    public TimeSpan? CheckOutTime { get; init; }
    public Guid StatusId { get; init; }
    public string? StatusNameEn { get; init; }
    public string? StatusNameAr { get; init; }
    public string? DeviceId { get; init; }
    public string? CheckInDeviceId { get; init; }
    public string? CheckOutDeviceId { get; init; }
    public TimeSpan? WorkedHours { get; init; }
    public TimeSpan? OvertimeHours { get; init; }
    public TimeSpan? LateMinutes { get; init; }
    public TimeSpan? EarlyLeaveMinutes { get; init; }
    public bool IsLate { get; init; }
    public bool IsEarlyLeave { get; init; }
    public bool IsOvertime { get; init; }
    public string? HalfDayRule { get; init; }
    public string? Notes { get; init; }
    public string? ApprovedBy { get; init; }
    public DateTime? ApprovedDate { get; init; }
}
