using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Payroll;

/// <summary>
/// Maintains employee salary structure and compensation history.
/// This entity tracks salary changes over time, supports salary revision processes, and serves as
/// the foundation for payroll calculations. Essential for compensation analysis, budget planning,
/// ensuring pay equity, and maintaining accurate payroll records. Enables historical salary tracking
/// for audits and salary increment reviews.
/// </summary>
public class Salary : BaseAuditableEntity
{
    public Guid EmployeeId { get; set; }
    public decimal BasicSalary { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Notes { get; set; }
    public bool IsCurrent { get; set; } = true;
    public string Currency { get; set; } = "EGP";
    public bool IsSocialInsuranceEnabled { get; set; }
    public decimal? SocialInsuranceEmployeeRate { get; set; }
    public decimal? SocialInsuranceEmployerRate { get; set; }
    public string? PaymentMethod { get; set; }
    public string? BankName { get; set; }
    public string? BankBranch { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankIban { get; set; }
    public string? BankSwiftCode { get; set; }

    /// <summary>
    /// Employee-specific overtime multiplier (e.g. 1.5, 2.0, 3.0).
    /// Overrides the OvertimeType default when calculating overtime pay.
    /// </summary>
    public decimal OvertimeMultiplier { get; set; } = 1.5m;

    // Navigation Properties
    public virtual Employee.Employee Employee { get; set; } = null!;
    public virtual ICollection<SalaryAllowance> Allowances { get; set; } = new List<SalaryAllowance>();
    public virtual ICollection<SalaryDeduction> Deductions { get; set; } = new List<SalaryDeduction>();
}
