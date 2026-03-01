namespace HrSystem.Application.Features.Payroll.Queries.GetPayrollHistory;

public class PayrollHistoryListItemDto
{
    public Guid PayslipId { get; set; }
    public Guid PayrollCycleId { get; set; }

    // Employee identification
    public Guid EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeNameEn { get; set; } = string.Empty;
    public string EmployeeNameAr { get; set; } = string.Empty;
    public string DepartmentNameEn { get; set; } = string.Empty;
    public string DepartmentNameAr { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }

    // Period
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public string PeriodLabel { get; set; } = string.Empty;

    // Salary components
    public decimal BasicSalary { get; set; }
    public decimal TotalAllowances { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal IncomeTax { get; set; }
    public decimal SocialInsuranceEmployee { get; set; }
    public decimal OvertimeAmount { get; set; }
    public decimal BonusAmount { get; set; }
    public decimal LeaveDeductions { get; set; }
    public int UnpaidLeaveDays { get; set; }
    public decimal NetSalary { get; set; }
    public string Currency { get; set; } = string.Empty;

    // Working days
    public int TotalWorkingDays { get; set; }
    public int ActualWorkingDays { get; set; }
    public int AbsentDays { get; set; }

    // Status
    public bool IsPaid { get; set; }
    public DateTime? PaidDate { get; set; }
    public DateTime? GeneratedDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? PdfFileUrl { get; set; }
}
