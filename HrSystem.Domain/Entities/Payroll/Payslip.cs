using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Payroll;

public class Payslip : BaseAuditableEntity
{
    public Guid PayrollCycleId { get; set; }
    public Guid EmployeeId { get; set; }
    public string PayslipNumber { get; set; } = string.Empty;
    
    // Salary Components
    public decimal BasicSalary { get; set; }
    public decimal TotalAllowances { get; set; }
    public decimal GrossSalary { get; set; }
    
    // Deductions
    public decimal TotalDeductions { get; set; }
    public decimal IncomeTax { get; set; }
    public decimal SocialInsuranceEmployee { get; set; }
    public decimal SocialInsuranceEmployer { get; set; }
    
    // Overtime & Bonuses
    public decimal OvertimeAmount { get; set; }
    public decimal BonusAmount { get; set; }
    
    // Leave Impact
    public decimal LeaveDeductions { get; set; }
    public int UnpaidLeaveDays { get; set; }
    
    // Final Amount
    public decimal NetSalary { get; set; }
    
    // Working Days
    public int TotalWorkingDays { get; set; }
    public int ActualWorkingDays { get; set; }
    public int AbsentDays { get; set; }
    
    // Document
    public string? PdfFilePath { get; set; }
    public string? PdfFileUrl { get; set; }
    public DateTime? GeneratedDate { get; set; }
    public bool IsPaid { get; set; }
    public DateTime? PaidDate { get; set; }

    // Navigation Properties
    public virtual PayrollCycle PayrollCycle { get; set; } = null!;
    public virtual Employee.Employee Employee { get; set; } = null!;
    public virtual ICollection<PayslipAllowance> PayslipAllowances { get; set; } = new List<PayslipAllowance>();
    public virtual ICollection<PayslipDeduction> PayslipDeductions { get; set; } = new List<PayslipDeduction>();
}
