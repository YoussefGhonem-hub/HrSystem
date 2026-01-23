using ErrorOr;
using HrSystem.Application.Features.Attendance.Queries.GetAttendanceById;
using HrSystem.Domain.Enums;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Attendance.Commands.VerifyBiometricAttendance;

public record VerifyBiometricAttendanceCommand(
    Guid? EmployeeId,
    BiometricType BiometricType,
    AttendancePunchType PunchType,
    string TemplateBase64,
    string? DeviceId,
    DateTime? EventTime
) : IRequest<ErrorOr<GenericResponse<AttendanceDto>>>;
