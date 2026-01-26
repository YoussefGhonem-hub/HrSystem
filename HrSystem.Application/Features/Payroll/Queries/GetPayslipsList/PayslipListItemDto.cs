namespace HrSystem.Application.Features.Payroll.Queries.GetPayslipsList;

public class PayslipListItemDto
{
    public Guid PayslipId { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetSalary { get; set; }
    public bool IsPaid { get; set; }
    public DateTime? GeneratedDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public string? PdfFileUrl { get; set; }
}
