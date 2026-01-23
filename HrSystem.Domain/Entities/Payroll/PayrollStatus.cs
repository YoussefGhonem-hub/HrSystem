using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Payroll;

/// <summary>
/// Represents the status of a payroll record
/// </summary>
public class PayrollStatus : BaseEntity
{
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ColorCode { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }

    // Navigation Properties
    public virtual ICollection<PayrollCycle> PayrollCycles { get; set; } = new List<PayrollCycle>();
}
