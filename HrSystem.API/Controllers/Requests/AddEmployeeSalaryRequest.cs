namespace HrSystem.API.Controllers.Requests;

public class AddEmployeeSalaryRequest
{
    public decimal BasicSalary { get; set; }
    public DateTime EffectiveDate { get; set; }
    public string? Notes { get; set; }
}
