using HrSystem.Domain.Common;
using HrSystem.Domain.Enums;

namespace HrSystem.Domain.Entities.Employee;

/// <summary>
/// Defines employee document types for categorization and validation.
/// </summary>
public class EmployeeDocumentType : BaseEntity
{
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DocumentCategory CategoryKey { get; set; } = DocumentCategory.Other;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public virtual ICollection<EmployeeDocument> Documents { get; set; } = new List<EmployeeDocument>();
}
