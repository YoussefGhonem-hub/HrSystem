using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Payroll;

public class DeductionType : BaseAuditableEntity
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsRecurring { get; set; } = true;

    // Navigation Properties
    public virtual ICollection<SalaryDeduction> SalaryDeductions { get; set; } = new List<SalaryDeduction>();
}
