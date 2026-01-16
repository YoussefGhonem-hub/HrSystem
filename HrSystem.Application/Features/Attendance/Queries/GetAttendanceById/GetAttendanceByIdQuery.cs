using ErrorOr;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Attendance.Queries.GetAttendanceById;

public record GetAttendanceByIdQuery(Guid Id) : IRequest<ErrorOr<GenericResponse<AttendanceDto>>>;
