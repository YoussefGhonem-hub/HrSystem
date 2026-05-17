namespace HrSystem.Domain.Enums;

/// <summary>
/// Defines how a payroll cycle's start and end dates are calculated.
/// </summary>
public enum PayCycleType
{
    /// <summary>
    /// Standard calendar month (1st → last day of month).
    /// </summary>
    MonthlyCalendar = 1,

    /// <summary>
    /// Custom cut-off dates set by the organization (e.g., 26th → 25th of next month).
    /// </summary>
    MonthlyCustomCutoff = 2,

    /// <summary>
    /// Every 14 days — 26 pay periods per year.
    /// </summary>
    BiWeekly = 3,

    /// <summary>
    /// Every 7 days — 52 pay periods per year.
    /// </summary>
    Weekly = 4
}
