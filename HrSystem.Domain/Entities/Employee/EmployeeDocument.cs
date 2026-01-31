using HrSystem.Domain.Common;
using HrSystem.Domain.Enums;

namespace HrSystem.Domain.Entities.Employee;

/// <summary>
/// Stores and manages employee-related documents digitally.
/// This entity enables paperless HR operations by securely storing contracts, certificates, IDs,
/// and other important documents. Facilitates compliance with document retention policies, enables
/// quick document retrieval during audits, and supports remote access to employee records.
/// Essential for regulatory compliance and efficient document management.
/// </summary>
public class EmployeeDocument : BaseAuditableEntity
{
    public Guid EmployeeId { get; set; }
    public EmployeeDocumentType DocumentType { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? FileUrl { get; set; }
    public string? Description { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;

    // Navigation Properties
    public virtual Employee Employee { get; set; } = null!;
}
