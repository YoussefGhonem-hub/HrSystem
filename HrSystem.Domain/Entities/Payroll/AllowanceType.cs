using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Payroll;

/// <summary>
/// Defines standardized allowance categories for employee compensation.
/// This entity ensures consistent allowance classification (housing, transportation, etc.),
/// facilitates tax and insurance calculations by defining allowance taxability rules,
/// and supports compliance with compensation regulations. Essential for standardized
/// benefit administration and accurate payroll processing.
/// </summary>
public class AllowanceType : BaseAuditableEntity
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsTaxable { get; set; } = true;
    public bool IsSubjectToInsurance { get; set; } = true;

    // Navigation Properties
    public virtual ICollection<SalaryAllowance> SalaryAllowances { get; set; } = new List<SalaryAllowance>();
}
