using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Employee;

/// <summary>
/// Defines standardized job positions and their salary ranges within the organization.
/// This entity ensures consistent job classification, facilitates salary benchmarking, supports career
/// progression planning, and enables structured compensation management. Critical for maintaining pay
/// equity, planning recruitment budgets, and establishing clear career paths for employees.
/// </summary>
public class JobTitle : BaseAuditableEntity
{
    public string TitleAr { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Level { get; set; }
    public decimal MinSalary { get; set; }
    public decimal MaxSalary { get; set; }

    // Branch scope
    public Guid? BranchId { get; set; }
    public virtual Organization.Branch? Branch { get; set; }

    // Navigation properties
    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
