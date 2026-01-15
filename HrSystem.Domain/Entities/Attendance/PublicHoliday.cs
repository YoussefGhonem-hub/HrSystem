using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Attendance;

/// <summary>
/// Maintains a calendar of official public holidays.
/// This entity ensures accurate attendance and payroll processing by identifying non-working days,
/// supports compliance with local labor laws regarding holiday pay, and helps in leave planning
/// and workforce scheduling. Essential for calculating working days in payroll cycles and
/// managing employee expectations around time off.
/// </summary>
public class PublicHoliday : BaseAuditableEntity
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int Year { get; set; }
    public bool IsRecurring { get; set; } // For annual holidays like Christmas
    public string? Description { get; set; }
}
