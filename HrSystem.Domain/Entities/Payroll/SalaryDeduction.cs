using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Payroll;

public class SalaryDeduction : BaseAuditableEntity
{
    public Guid SalaryId { get; set; }
    public Guid DeductionTypeId { get; set; }
    public decimal Amount { get; set; }
    public bool IsPercentage { get; set; } = false;
    public decimal? PercentageValue { get; set; }

    // Navigation Properties
    public virtual Salary Salary { get; set; } = null!;
    public virtual DeductionType DeductionType { get; set; } = null!;
}
