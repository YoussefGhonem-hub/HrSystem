using ErrorOr;
using HrSystem.Application.Features.Attendance.Queries.GetAttendanceById;
using HrSystem.Domain.Enums;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Attendance.Commands.CheckInOut;

/// <summary>
/// Unified check-in/check-out command that supports multiple attendance methods
/// and validates against branch attendance settings and check-in points.
/// </summary>
public record CheckInOutCommand(
    Guid? EmployeeId,
    AttendancePunchType PunchType,
    AttendanceMethod Method,

    // Location data (required for Location method, optional for FaceId)
    double? Latitude,
    double? Longitude,

    // FaceId data (required for FaceId method)
    string? FaceTemplateBase64,

    // Device/fingerprint info
    string? DeviceId,

    // Event time (defaults to now)
    DateTime? EventTime,

    string? Notes
) : IRequest<ErrorOr<GenericResponse<AttendanceDto>>>;
