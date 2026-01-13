using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Employee;

public class Department : BaseAuditableEntity
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ManagerId { get; set; }
    public Guid? ParentDepartmentId { get; set; }

    // Navigation properties
    public virtual Employee? Manager { get; set; }
    public virtual Department? ParentDepartment { get; set; }
    public virtual ICollection<Department> SubDepartments { get; set; } = new List<Department>();
    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
