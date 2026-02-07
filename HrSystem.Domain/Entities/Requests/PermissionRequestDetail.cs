using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Requests;

/// <summary>
/// Type-specific details for Permission requests (leave early, come late, short absence).
/// This is for hours-based permissions, NOT full-day vacations.
/// </summary>
public class PermissionRequestDetail : BaseAuditableEntity
{
    public Guid EmployeeRequestId { get; set; }
    
    // Permission type from master
    public Guid PermissionTypeId { get; set; }
    
    /// <summary>
    /// The specific date for this permission
    /// </summary>
    public DateTime PermissionDate { get; set; }
    
    /// <summary>
    /// Time when employee needs to leave early or will arrive late
    /// </summary>
    public TimeSpan? FromTime { get; set; }
    
    /// <summary>
    /// Time when employee will return or expected arrival
    /// </summary>
    public TimeSpan? ToTime { get; set; }
    
    /// <summary>
    /// Total hours of permission requested
    /// </summary>
    public decimal TotalHours { get; set; }
    
    /// <summary>
    /// Reason for the permission request
    /// </summary>
    public string Reason { get; set; } = string.Empty;
    
    // Approval workflow
    public Guid? ManagerId { get; set; }
    public DateTime? ManagerApprovalDate { get; set; }
    public string? ManagerComments { get; set; }
    public string? RejectionReason { get; set; }
    
    /// <summary>
    /// If permission deducts from leave, track the fraction deducted
    /// </summary>
    public decimal? LeaveDeduction { get; set; }
    
    // Navigation
    public virtual EmployeeRequest EmployeeRequest { get; set; } = null!;
    public virtual PermissionType PermissionType { get; set; } = null!;
    public virtual Employee.Employee? Manager { get; set; }
}
