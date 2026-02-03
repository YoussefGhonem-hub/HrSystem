using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Payroll;

/// <summary>
/// Links specific deductions to employee salary structures.
/// This entity manages individual employee deductions such as loan repayments or special deductions,
/// supports both fixed and percentage-based deductions, and ensures accurate net salary calculations.
/// Critical for maintaining transparent deduction records and ensuring compliance with employment
/// agreements regarding salary deductions.
/// </summary>
public class SalaryDeduction : BaseAuditableEntity
{
    public Guid SalaryId { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsRecurring { get; set; } = true;
    public decimal Amount { get; set; }
    public bool IsPercentage { get; set; } = false;
    public decimal? PercentageValue { get; set; }

    // Navigation Properties
    public virtual Salary Salary { get; set; } = null!;
}
