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
    public string Code { get; set; } = string.Empty; // Unique code per organization
    public string? Description { get; set; }
    public int Level { get; set; }
    public decimal MinSalary { get; set; }
    public decimal MaxSalary { get; set; }
    
    // Organization Reference (for multi-tenant isolation)
    public Guid OrganizationId { get; set; }
    
    // Status
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;

    public virtual Organization.Branch? Branch { get; set; }
    public virtual Organization.Organization Organization { get; set; } = null!;

    // Navigation properties
    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
