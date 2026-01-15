using Microsoft.AspNetCore.Identity;

namespace HrSystem.Domain.Entities.Account;

/// <summary>
/// Defines user roles and permissions within the HR system.
/// This entity enables role-based access control (RBAC) to ensure users only access features and data
/// relevant to their responsibilities (e.g., HR Admin, Manager, Employee). Helps maintain security,
/// compliance, and proper segregation of duties across the organization.
/// </summary>
public class ApplicationRole : IdentityRole<Guid>
{
    public string? DisplayName { get; set; }
}
