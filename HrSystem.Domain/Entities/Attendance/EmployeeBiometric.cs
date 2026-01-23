using HrSystem.Domain.Common;
using HrSystem.Domain.Enums;

namespace HrSystem.Domain.Entities.Attendance;

public class EmployeeBiometric : BaseAuditableEntity
{
    public Guid EmployeeId { get; set; }
    public BiometricType BiometricType { get; set; }
    public string TemplateHash { get; set; } = string.Empty;
    public string? TemplateData { get; set; }
    public string? Provider { get; set; }
    public string? DeviceId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset EnrolledAt { get; set; } = DateTimeOffset.UtcNow;

    public virtual Employee.Employee Employee { get; set; } = null!;
}
