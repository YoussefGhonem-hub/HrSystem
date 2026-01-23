using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Employee;

/// <summary>
/// Represents employee employment status
/// </summary>
public class EmployeeStatus : BaseEntity
{
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ColorCode { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }

    // Navigation Properties
    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
