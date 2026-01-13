using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Payroll;

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
