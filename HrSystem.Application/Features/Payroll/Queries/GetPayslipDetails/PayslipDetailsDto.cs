using HrSystem.Application.Features.Payroll.Queries.GetMyPayslipDetails;

namespace HrSystem.Application.Features.Payroll.Queries.GetPayslipDetails;

public class PayslipDetailsDto
{
    public Guid PayslipId { get; set; }
    public Guid PayrollCycleId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public string PayslipNumber { get; set; } = string.Empty;

    // Employee Info
    public Guid EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeNameEn { get; set; } = string.Empty;
    public string EmployeeNameAr { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;

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
    public string? PdfFileUrl { get; set; }
    public DateTime? GeneratedDate { get; set; }
    public bool IsPaid { get; set; }
    public DateTime? PaidDate { get; set; }

    // Breakdown
    public List<MyPayslipAllowanceDto> Allowances { get; set; } = new();
    public List<MyPayslipDeductionDto> Deductions { get; set; } = new();
}
