using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Employee;

public class JobTitle : BaseAuditableEntity
{
    public string TitleAr { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Level { get; set; }
    public decimal MinSalary { get; set; }
    public decimal MaxSalary { get; set; }

    // Navigation properties
    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
