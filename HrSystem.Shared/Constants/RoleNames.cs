namespace HrSystem.Shared.Constants;

/// <summary>
/// Constants for role names used throughout the application
/// </summary>
public static class RoleNames
{
    /// <summary>
    /// Super Administrator role with full system access
    /// </summary>
    public const string SuperAdmin = "SuperAdmin";

    /// <summary>
    /// Administrator role with full system access
    /// </summary>
    public const string Admin = "Admin";

    /// <summary>
    /// HR Manager role with HR management capabilities
    /// </summary>
    public const string HRManager = "HR Manager";

    /// <summary>
    /// Department Manager role with department management capabilities
    /// </summary>
    public const string DepartmentManager = "Department Manager";

    /// <summary>
    /// General Manager role
    /// </summary>
    public const string Manager = "Manager";

    /// <summary>
    /// Regular employee role
    /// </summary>
    public const string Employee = "Employee";

    /// <summary>
    /// Supervisor role
    /// </summary>
    public const string Supervisor = "Supervisor";

    /// <summary>
    /// Team Lead role
    /// </summary>
    public const string TeamLead = "Team Lead";

    /// <summary>
    /// Gets all available role names
    /// </summary>
    public static readonly string[] All = new[]
    {
        SuperAdmin,
        Admin,
        HRManager,
        DepartmentManager,
        Manager,
        Employee,
        Supervisor,
        TeamLead
    };

    /// <summary>
    /// Checks if a role name is valid
    /// </summary>
    public static bool IsValid(string roleName)
    {
        return All.Contains(roleName, StringComparer.OrdinalIgnoreCase);
    }
}