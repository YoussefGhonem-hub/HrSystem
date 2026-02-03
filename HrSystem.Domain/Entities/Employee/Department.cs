using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Employee;

/// <summary>
/// Represents organizational departments and their hierarchical structure.
/// This entity enables effective organizational structure management, supporting hierarchical relationships
/// between departments. Facilitates proper reporting structures, budget allocation, resource planning,
/// and performance tracking at the departmental level. Essential for organizational clarity and efficient
/// workforce management across different business units.
/// </summary>
public class Department : BaseAuditableEntity
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty; // Unique code per organization
    public string? Description { get; set; }
    public Guid? ManagerId { get; set; }
    public Guid? ParentDepartmentId { get; set; }
    
    // Organization Reference (for multi-tenant isolation)
    public Guid OrganizationId { get; set; }
    
    // Status
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;

    // Navigation properties
    public virtual Employee? Manager { get; set; }
    public virtual Department? ParentDepartment { get; set; }
    public virtual Organization.Branch? Branch { get; set; }
    public virtual Organization.Organization Organization { get; set; } = null!;
    public virtual ICollection<Department> SubDepartments { get; set; } = new List<Department>();
    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
