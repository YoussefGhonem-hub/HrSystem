using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Payroll;

public class TaxBracket : BaseAuditableEntity
{
    public int Year { get; set; }
    public decimal MinIncome { get; set; }
    public decimal MaxIncome { get; set; }
    public decimal TaxRate { get; set; }
    public decimal FixedAmount { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }
}
