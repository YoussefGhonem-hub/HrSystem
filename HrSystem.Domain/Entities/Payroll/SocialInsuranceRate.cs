using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Payroll;

/// <summary>
/// Maintains current social insurance contribution rates and regulations.
/// This entity ensures compliance with government-mandated social insurance requirements,
/// enables accurate calculation of employee and employer contributions, and adapts to regulatory
/// changes over time. Critical for legal compliance, avoiding penalties, and ensuring proper
/// employee social security coverage.
/// </summary>
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
