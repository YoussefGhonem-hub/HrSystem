using HrSystem.Domain.Common;
using HrSystem.Domain.Enums;

namespace HrSystem.Domain.Entities.Payroll;

public class PayrollCycle : BaseAuditableEntity
{
    public string CycleName { get; set; } = string.Empty; // e.g., "January 2026"
    public int Month { get; set; }
    public int Year { get; set; }
    public DateTime PeriodStartDate { get; set; }
    public DateTime PeriodEndDate { get; set; }
    public DateTime? PaymentDate { get; set; }
    public PayrollStatus Status { get; set; }
    public decimal TotalGrossSalary { get; set; }
    public decimal TotalNetSalary { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalTax { get; set; }
    public decimal TotalInsurance { get; set; }
    public string? Notes { get; set; }

    // Navigation Properties
    public virtual ICollection<Payslip> Payslips { get; set; } = new List<Payslip>();
}
