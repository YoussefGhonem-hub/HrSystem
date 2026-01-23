using ErrorOr;
using HrSystem.Domain.Enums;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Attendance.Commands.EnrollEmployeeBiometric;

public record EnrollEmployeeBiometricCommand(
    Guid EmployeeId,
    BiometricType BiometricType,
    string TemplateBase64,
    string? Provider,
    string? DeviceId,
    bool IsActive = true
) : IRequest<ErrorOr<GenericResponse<EmployeeBiometricDto>>>;
