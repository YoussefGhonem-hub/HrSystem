using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Attendance;

/// <summary>
/// Represents the status of an attendance record
/// </summary>
public class AttendanceStatus : BaseEntity
{
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ColorCode { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }

    // Navigation Properties
    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
}
