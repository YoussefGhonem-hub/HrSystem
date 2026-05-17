using HrSystem.Domain.Common;
using HrSystem.Domain.Enums;

namespace HrSystem.Domain.Entities.Organization;

/// <summary>
/// Stores organization-level payroll cycle configuration.
/// One record per organization — upserted on save.
/// </summary>
public class OrganizationPayrollSettings : BaseEntity
{
    /// <summary>
    /// How payroll periods are calculated.
    /// </summary>
    public PayCycleType CycleType { get; set; } = PayCycleType.MonthlyCalendar;

    /// <summary>
    /// For <see cref="PayCycleType.MonthlyCustomCutoff"/>: day of month the period starts (1–28).
    /// </summary>
    public int? CustomCutoffStartDay { get; set; }

    /// <summary>
    /// For <see cref="PayCycleType.MonthlyCustomCutoff"/>: day of month the period ends (1–28).
    /// </summary>
    public int? CustomCutoffEndDay { get; set; }

    /// <summary>
    /// For <see cref="PayCycleType.BiWeekly"/> and <see cref="PayCycleType.Weekly"/>:
    /// the date that anchors the first period start.
    /// </summary>
    public DateOnly? AnchorDate { get; set; }

    /// <summary>
    /// Optional notes visible only to HR/Admin.
    /// </summary>
    public string? Notes { get; set; }

    // Navigation
    public virtual Organization Organization { get; set; } = null!;
}
