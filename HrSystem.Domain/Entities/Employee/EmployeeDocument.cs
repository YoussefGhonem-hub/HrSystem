using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Employee;

public class EmployeeDocument : BaseAuditableEntity
{
    public Guid EmployeeId { get; set; }
    public string DocumentType { get; set; } = string.Empty; // ID, Contract, Insurance, Certificate, etc.
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
