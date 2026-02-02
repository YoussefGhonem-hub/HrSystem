using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Requests;

/// <summary>
/// Master data source for dropdowns associated with self-service requests.
/// Example: Vacation types, training programs, miscellaneous categories.
/// </summary>
public class EmployeeRequestOption : BaseAuditableEntity
{
    public Guid RequestTypeId { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public bool RequiresAttachment { get; set; }
    public bool RequiresManagerApproval { get; set; }
    public bool IsDefault { get; set; }
    public int SortOrder { get; set; } = 1;

    public virtual ICollection<EmployeeRequest> Requests { get; set; } = new List<EmployeeRequest>();
    public virtual RequestType? RequestTypeRef { get; set; }
}
