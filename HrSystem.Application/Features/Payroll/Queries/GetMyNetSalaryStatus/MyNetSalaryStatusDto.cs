namespace HrSystem.Application.Features.Payroll.Queries.GetMyNetSalaryStatus;

public class MyNetSalaryStatusDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal NetSalary { get; set; }
    public bool IsPaid { get; set; }
    public string Status => IsPaid ? "Paid" : "Unpaid";
}
