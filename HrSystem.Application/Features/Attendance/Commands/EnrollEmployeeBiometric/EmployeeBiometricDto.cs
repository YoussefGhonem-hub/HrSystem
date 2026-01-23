using HrSystem.Domain.Enums;

namespace HrSystem.Application.Features.Attendance.Commands.EnrollEmployeeBiometric;

public class EmployeeBiometricDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public BiometricType BiometricType { get; set; }
    public string? Provider { get; set; }
    public string? DeviceId { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset EnrolledAt { get; set; }
}
