using Microsoft.AspNetCore.Identity;

namespace HrSystem.Domain.Entities.Account;

public class ApplicationUser : IdentityUser<Guid>
{
    public string? FullName { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Multi-Tenancy
    public Guid OrganizationId { get; set; }
    
    // Employee Reference
    public Guid? EmployeeId { get; set; }
    
    // Audit
    public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.UtcNow;
    public Guid? CreatedBy { get; set; }
    public DateTimeOffset? ModifiedDate { get; set; }
    public Guid? ModifiedBy { get; set; }
    
    // Navigation Properties
    public virtual Organization.Organization Organization { get; set; } = null!;
    public virtual Employee.Employee? Employee { get; set; }
}
