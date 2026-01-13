using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Payroll;

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
