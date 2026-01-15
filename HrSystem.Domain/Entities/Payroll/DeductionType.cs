using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Payroll;

/// <summary>
/// Defines standardized deduction categories for payroll processing.
/// This entity ensures consistent handling of salary deductions (loans, advances, penalties),
/// distinguishes between recurring and one-time deductions, and maintains compliance with
/// deduction regulations. Critical for accurate net salary calculations and proper documentation
/// of all salary adjustments.
/// </summary>
public class DeductionType : BaseAuditableEntity
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsRecurring { get; set; } = true;

    // Navigation Properties
    public virtual ICollection<SalaryDeduction> SalaryDeductions { get; set; } = new List<SalaryDeduction>();
}
