using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Requests;

/// <summary>
/// Type-specific details for Miscellaneous requests.
/// Links to MiscellaneousType master.
/// </summary>
public class MiscellaneousRequestDetail : BaseAuditableEntity
{
    public Guid EmployeeRequestId { get; set; }
    
    // Miscellaneous type from master
    public Guid MiscellaneousTypeId { get; set; }
    public string? AdditionalNotes { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Priority { get; set; }
    public DateTime? ExpectedCompletionDate { get; set; }
    
    // Navigation
    public virtual EmployeeRequest EmployeeRequest { get; set; } = null!;
    public virtual MiscellaneousType MiscellaneousType { get; set; } = null!;
}
