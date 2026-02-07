using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Requests;

/// <summary>
/// Type-specific details for Vacation requests (annual leave, sick leave, etc.).
/// Links to VacationType master for dropdown selection.
/// </summary>
public class VacationRequestDetail : BaseAuditableMasterEntity
{
    public Guid EmployeeRequestId { get; set; }
    
    // Vacation type from master (for dropdown selection)
    public Guid VacationTypeId { get; set; }
    public decimal TotalDays { get; set; }
    
    // Approval workflow
    public Guid? ManagerId { get; set; }
    public DateTime? ManagerApprovalDate { get; set; }
    public string? ManagerComments { get; set; }
    
    // HR Approval
    public Guid? HRApprovedBy { get; set; }
    public DateTime? HRApprovalDate { get; set; }
    public string? HRComments { get; set; }
    
    public string? RejectionReason { get; set; }
    
    // Emergency contact for long vacations
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    
    // Navigation
    public virtual EmployeeRequest EmployeeRequest { get; set; } = null!;
    public virtual VacationType VacationType { get; set; } = null!;
    public virtual Employee.Employee? Manager { get; set; }
}
