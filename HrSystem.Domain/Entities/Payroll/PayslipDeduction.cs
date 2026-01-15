using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Payroll;

/// <summary>
/// Captures specific deduction amounts on individual payslips.
/// This entity provides detailed breakdown of all deductions applied in a specific period, ensuring
/// transparency in salary reductions and supporting employee understanding of net pay calculations.
/// Critical for maintaining trust and resolving any disputes regarding salary deductions.
/// </summary>
public class PayslipDeduction : BaseEntity
{
    public Guid PayslipId { get; set; }
    public Guid DeductionTypeId { get; set; }
    public string DeductionNameAr { get; set; } = string.Empty;
    public string DeductionNameEn { get; set; } = string.Empty;
    public decimal Amount { get; set; }

    // Navigation Properties
    public virtual Payslip Payslip { get; set; } = null!;
    public virtual DeductionType DeductionType { get; set; } = null!;
}
