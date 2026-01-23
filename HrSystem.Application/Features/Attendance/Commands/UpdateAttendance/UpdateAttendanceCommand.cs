using ErrorOr;
using HrSystem.Application.Features.Attendance.Queries.GetAttendanceById;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Attendance.Commands.UpdateAttendance;

public record UpdateAttendanceCommand(
    Guid Id,
    TimeSpan? CheckInTime,
    TimeSpan? CheckOutTime,
    Guid StatusId,
    string? DeviceId,
    string? CheckInDeviceId,
    string? CheckOutDeviceId,
    TimeSpan? OvertimeHours,
    TimeSpan? LateMinutes,
    TimeSpan? EarlyLeaveMinutes,
    bool IsLate,
    bool IsEarlyLeave,
    bool IsOvertime,
    string? Notes,
    string? ApprovedBy
) : IRequest<ErrorOr<GenericResponse<AttendanceDto>>>;
