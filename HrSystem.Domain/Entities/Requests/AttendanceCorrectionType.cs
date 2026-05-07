using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Requests;

/// <summary>
/// Master data for attendance correction sub-types.
/// </summary>
public class AttendanceCorrectionType : BaseAuditableMasterEntity
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool RequiresManagerApproval { get; set; }
    public bool RequireAttachment { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 1;
}
