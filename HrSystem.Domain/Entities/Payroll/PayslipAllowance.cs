using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Payroll;

/// <summary>
/// Captures specific allowance amounts on individual payslips.
/// This entity provides detailed breakdown of all allowances paid in a specific period, enabling
/// transparent compensation reporting and facilitating employee understanding of their total earnings.
/// Essential for payslip clarity and supporting queries about compensation components.
/// </summary>
public class PayslipAllowance : BaseEntity
{
    public Guid PayslipId { get; set; }
    public Guid AllowanceTypeId { get; set; }
    public string AllowanceNameAr { get; set; } = string.Empty;
    public string AllowanceNameEn { get; set; } = string.Empty;
    public decimal Amount { get; set; }

    // Navigation Properties
    public virtual Payslip Payslip { get; set; } = null!;
    public virtual AllowanceType AllowanceType { get; set; } = null!;
}
