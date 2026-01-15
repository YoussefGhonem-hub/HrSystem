using HrSystem.Domain.Common;
using HrSystem.Domain.Enums;

namespace HrSystem.Domain.Entities.Leave;

/// <summary>
/// Manages employee leave applications and approval workflows.
/// This entity streamlines the leave request process with multi-level approvals, ensures adequate
/// documentation, and maintains compliance with leave policies. Enables workforce planning by
/// providing visibility into upcoming absences, prevents scheduling conflicts, and maintains
/// accurate records for payroll deductions and leave balance updates.
/// </summary>
public class LeaveRequest : BaseAuditableEntity
{
    public Guid EmployeeId { get; set; }
    public Guid LeavePolicyId { get; set; }
    public LeaveType LeaveType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalDays { get; set; }
    public string Reason { get; set; } = string.Empty;
    public LeaveStatus Status { get; set; }
    
    // Approval Workflow
    public Guid? ManagerId { get; set; }
    public DateTime? ManagerApprovalDate { get; set; }
    public string? ManagerComments { get; set; }
    
    public Guid? HRApprovedBy { get; set; }
    public DateTime? HRApprovalDate { get; set; }
    public string? HRComments { get; set; }
    
    public string? RejectionReason { get; set; }
    
    // Documents
    public string? DocumentPath { get; set; }
    public string? DocumentUrl { get; set; }
    
    // Emergency Contact (for long leaves)
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }

    // Navigation Properties
    public virtual Employee.Employee Employee { get; set; } = null!;
    public virtual LeavePolicy LeavePolicy { get; set; } = null!;
    public virtual Employee.Employee? Manager { get; set; }
}
