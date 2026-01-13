using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Attendance;

public class PublicHoliday : BaseAuditableEntity
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int Year { get; set; }
    public bool IsRecurring { get; set; } // For annual holidays like Christmas
    public string? Description { get; set; }
}
