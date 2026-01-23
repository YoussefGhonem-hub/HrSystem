namespace HrSystem.Application.Features.Payroll.Queries.GetMyPayslips;

public class MyPayslipListItemDto
{
    public Guid PayslipId { get; set; }
    public Guid PayrollCycleId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal NetSalary { get; set; }
    public string? PdfFileUrl { get; set; }
    public DateTime? GeneratedDate { get; set; }
}
