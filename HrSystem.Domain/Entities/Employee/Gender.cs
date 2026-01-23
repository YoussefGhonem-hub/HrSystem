using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Employee;

/// <summary>
/// Represents gender options
/// </summary>
public class Gender : BaseEntity
{
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }

    // Navigation Properties
    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
