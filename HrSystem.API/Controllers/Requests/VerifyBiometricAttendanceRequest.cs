using HrSystem.Domain.Enums;

namespace HrSystem.API.Controllers.Requests;

public class VerifyBiometricAttendanceRequest
{
    public Guid? EmployeeId { get; set; }
    public BiometricType BiometricType { get; set; }
    public AttendancePunchType PunchType { get; set; }
    public string TemplateBase64 { get; set; } = string.Empty;
    public string? DeviceId { get; set; }
    public DateTime? EventTime { get; set; }
}
