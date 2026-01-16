using ErrorOr;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Attendance.Commands.DeleteAttendance;

public record DeleteAttendanceCommand(Guid Id) : IRequest<ErrorOr<GenericResponse>>;
