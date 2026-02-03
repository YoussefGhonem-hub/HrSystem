using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Requests;

/// <summary>
/// Type-specific details for Vacation requests (annual leave, sick leave, etc.).
/// Links to VacationType master for dropdown selection.
/// Also links to LeaveType/LeavePolicy for balance tracking.
/// </summary>
public class VacationRequestDetail : BaseAuditableEntity
{
    public Guid EmployeeRequestId { get; set; }
    
    // Vacation type from master (for dropdown selection)
    public Guid VacationTypeId { get; set; }
    public decimal TotalDays { get; set; }
    
    // Link to Leave system for balance tracking (maps VacationType to LeaveType)
    public Guid? LeaveTypeId { get; set; }
    public Guid? LeavePolicyId { get; set; }
    
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
    public virtual Leave.LeaveType? LeaveType { get; set; }
    public virtual Leave.LeavePolicy? LeavePolicy { get; set; }
}
