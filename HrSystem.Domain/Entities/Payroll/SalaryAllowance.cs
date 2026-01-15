using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Payroll;

/// <summary>
/// Links specific allowances to employee salary structures.
/// This entity enables customized compensation packages by allowing different allowances
/// for different employees. Supports flexible allowance calculations (fixed amount or percentage),
/// ensures accurate gross salary computation, and facilitates transparent compensation breakdown
/// in payslips. Essential for competitive and equitable compensation management.
/// </summary>
public class SalaryAllowance : BaseAuditableEntity
{
    public Guid SalaryId { get; set; }
    public Guid AllowanceTypeId { get; set; }
    public decimal Amount { get; set; }
    public bool IsPercentage { get; set; } = false;
    public decimal? PercentageValue { get; set; }

    // Navigation Properties
    public virtual Salary Salary { get; set; } = null!;
    public virtual AllowanceType AllowanceType { get; set; } = null!;
}
