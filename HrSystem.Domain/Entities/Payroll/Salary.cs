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

    // Navigation Properties
    public virtual Employee.Employee Employee { get; set; } = null!;
    public virtual ICollection<SalaryAllowance> Allowances { get; set; } = new List<SalaryAllowance>();
    public virtual ICollection<SalaryDeduction> Deductions { get; set; } = new List<SalaryDeduction>();
}
