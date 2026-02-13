using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Requests;

/// <summary>
/// Master list of request categories replacing the EmployeeRequestType enum.
/// </summary>
public class RequestType : BaseAuditableMasterEntity
{
    public string Code { get; set; } = string.Empty; // e.g., Vacation, OverTime
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 1;
    public bool RequireAttachment { get; set; }
}
