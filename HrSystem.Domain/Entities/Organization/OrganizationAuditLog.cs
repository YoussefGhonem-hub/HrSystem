using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Organization;

public class OrganizationAuditLog : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public Guid? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string? OldValues { get; set; } // JSON
    public string? NewValues { get; set; } // JSON
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    // Navigation Properties
    public virtual Organization Organization { get; set; } = null!;
}
