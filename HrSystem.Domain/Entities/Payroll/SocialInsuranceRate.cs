using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Payroll;

public class SocialInsuranceRate : BaseAuditableEntity
{
    public int Year { get; set; }
    public decimal EmployeeRate { get; set; } // e.g., 14%
    public decimal EmployerRate { get; set; } // e.g., 18.75%
    public decimal MinSalaryBase { get; set; }
    public decimal MaxSalaryBase { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }
}
