using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Payroll;

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
