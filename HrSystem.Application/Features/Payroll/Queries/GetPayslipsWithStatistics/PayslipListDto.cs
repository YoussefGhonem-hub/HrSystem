namespace HrSystem.Application.Features.Payroll.Queries.GetPayslipsWithStatistics;

public class PayslipListDto
{
    public Guid Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeNameAr { get; set; } = string.Empty;
    public string EmployeeNameEn { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string PayslipNumber { get; set; } = string.Empty;
    public decimal BasicSalary { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetSalary { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public bool IsPaid { get; set; }
    public DateTime? PaidDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? PdfFileUrl { get; set; }
    public DateTime? GeneratedDate { get; set; }
}
