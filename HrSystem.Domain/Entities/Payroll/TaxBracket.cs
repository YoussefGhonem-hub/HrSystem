using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Payroll;

/// <summary>
/// Maintains progressive income tax rates based on government tax regulations.
/// This entity enables accurate tax calculation according to current tax laws, supports automatic
/// adaptation to tax law changes, and ensures compliance with tax withholding requirements.
/// Essential for proper tax deductions, avoiding legal issues, and maintaining accurate net salary
/// calculations for employees across different income levels.
/// </summary>
public class TaxBracket : BaseAuditableEntity
{
    public int Year { get; set; }
    public decimal MinIncome { get; set; }
    public decimal MaxIncome { get; set; }
    public decimal TaxRate { get; set; }
    public decimal FixedAmount { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }
}
