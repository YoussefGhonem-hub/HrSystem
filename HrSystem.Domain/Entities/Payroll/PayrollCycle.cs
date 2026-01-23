using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Payroll;

/// <summary>
/// Represents a complete payroll processing period (typically monthly).
/// This entity orchestrates the entire payroll process, aggregates payroll costs for financial reporting,
/// tracks processing status, and maintains payroll history. Essential for financial planning, budget
/// management, ensuring timely salary payments, and maintaining organized payroll records for audits
/// and compliance verification.
/// </summary>
public class PayrollCycle : BaseAuditableEntity
{
    public string CycleName { get; set; } = string.Empty; // e.g., "January 2026"
    public int Month { get; set; }
    public int Year { get; set; }
    public DateTime PeriodStartDate { get; set; }
    public DateTime PeriodEndDate { get; set; }
    public DateTime? PaymentDate { get; set; }
    public Guid StatusId { get; set; }
    public decimal TotalGrossSalary { get; set; }
    public decimal TotalNetSalary { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalTax { get; set; }
    public decimal TotalInsurance { get; set; }
    public string? Notes { get; set; }

    // Navigation Properties
    public virtual ICollection<Payslip> Payslips { get; set; } = new List<Payslip>();
    public virtual PayrollStatus Status { get; set; } = null!;
}
