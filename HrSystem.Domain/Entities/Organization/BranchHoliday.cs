using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Organization;

/// <summary>
/// Per-branch holiday definitions and overrides.
/// Allows each branch to have its own holiday calendar, accommodating
/// regional/country-specific holidays that differ by location.
/// </summary>
public class BranchHoliday : BaseAuditableEntity
{
    public new Guid BranchId { get; set; }
    
    // Holiday Details
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // Date Information
    public DateTime Date { get; set; }
    public int Year { get; set; }
    
    // Recurrence
    public bool IsRecurring { get; set; } = false; // true = repeats every year
    public int? RecurringMonth { get; set; } // 1-12 for recurring holidays
    public int? RecurringDay { get; set; }   // 1-31 for recurring holidays
    
    // Holiday Type
    public HolidayType Type { get; set; } = HolidayType.Public;
    
    // Status
    public bool IsActive { get; set; } = true;
    
    // Navigation Properties
    public virtual Branch Branch { get; set; } = null!;
}

/// <summary>
/// Types of holidays
/// </summary>
public enum HolidayType
{
    Public = 1,      // Government/National holiday
    Religious = 2,    // Religious observance
    Company = 3,      // Company-specific holiday
    Regional = 4      // Regional/Local holiday
}
