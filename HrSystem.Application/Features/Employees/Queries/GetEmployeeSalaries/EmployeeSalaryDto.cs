namespace HrSystem.Application.Features.Employees.Queries.GetEmployeeSalaries;

public class EmployeeSalaryDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public decimal BasicSalary { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Notes { get; set; }
    public bool IsCurrent { get; set; }
    public string Currency { get; set; } = string.Empty;
    public bool IncludeSocialInsurance { get; set; }
    public decimal? SocialInsuranceEmployeeRate { get; set; }
    public decimal? SocialInsuranceEmployerRate { get; set; }
    public string? PaymentMethod { get; set; }
}
