using HrSystem.Domain.Enums;

namespace HrSystem.API.Controllers.Requests;

public class EnrollEmployeeBiometricRequest
{
    public Guid EmployeeId { get; set; }
    public BiometricType BiometricType { get; set; }
    public string TemplateBase64 { get; set; } = string.Empty;
    public string? Provider { get; set; }
    public string? DeviceId { get; set; }
    public bool IsActive { get; set; } = true;
}
