using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Requests;

/// <summary>
/// Type-specific details for Personal requests.
/// Links to PersonalType master.
/// </summary>
public class PersonalRequestDetail : BaseAuditableMasterEntity
{
    public Guid EmployeeRequestId { get; set; }
    
    // Personal type from master
    public Guid PersonalTypeId { get; set; }
    public string? Reason { get; set; }
    public bool IsUrgent { get; set; }
    public bool RequiresConfidentiality { get; set; }
    public string? PreferredContactMethod { get; set; }
    public string? AdditionalContactInfo { get; set; }
    
    // Navigation
    public virtual EmployeeRequest EmployeeRequest { get; set; } = null!;
    public virtual PersonalType PersonalType { get; set; } = null!;
}
