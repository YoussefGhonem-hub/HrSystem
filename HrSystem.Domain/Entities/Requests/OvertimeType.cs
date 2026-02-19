using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Requests;

/// <summary>
/// Master data for Overtime types (Regular, Weekend, Holiday, Night Shift, etc.)
/// Used for dropdown selection when submitting overtime requests.
/// </summary>
public class OvertimeType : BaseAuditableMasterEntity
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal DefaultMultiplier { get; set; } = 1.5m;
    public bool RequiresManagerApproval { get; set; } = true;
    public bool RequireAttachment { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 1;
}
