using HrSystem.Domain.Enums;

namespace HrSystem.Application.Features.Attendance.Queries.GetAttendancesList;

public record AttendanceListDto
{
    public Guid Id { get; init; }
    public Guid EmployeeId { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public string EmployeeCode { get; init; } = string.Empty;
    public DateTime Date { get; init; }
    public TimeSpan? CheckInTime { get; init; }
    public TimeSpan? CheckOutTime { get; init; }
    public AttendanceStatus Status { get; init; }
    public TimeSpan? WorkedHours { get; init; }
    public bool IsLate { get; init; }
    public bool IsEarlyLeave { get; init; }
    public bool IsOvertime { get; init; }
}
