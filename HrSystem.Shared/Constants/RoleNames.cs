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
    /// Organization Administrator role with full organization access
    /// </summary>
    public const string OrganizationAdmin = "OrganizationAdmin";

    /// <summary>
    /// HR Manager role with HR management capabilities
    /// </summary>
    public const string HRManager = "HRManager";

    /// <summary>
    /// HR Specialist role with HR support capabilities
    /// </summary>
    public const string HRSpecialist = "HRSpecialist";

    /// <summary>
    /// Department Manager role with department management capabilities
    /// </summary>
    public const string DepartmentManager = "DepartmentManager";

    /// <summary>
    /// Regular employee role
    /// </summary>
    public const string Employee = "Employee";

    /// <summary>
    /// Roles that must always be scoped to a branch
    /// </summary>
    public static readonly string[] BranchScoped = new[]
    {
        HRManager,
        HRSpecialist,
        DepartmentManager,
        Employee
    };

    /// <summary>
    /// Gets all available role names
    /// </summary>
    public static readonly string[] All = new[]
    {
        SuperAdmin,
        OrganizationAdmin,
        HRManager,
        HRSpecialist,
        DepartmentManager,
        Employee
    };

    private static readonly Dictionary<string, string> AliasToCanonical = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Admin"] = OrganizationAdmin,
        ["Organization Admin"] = OrganizationAdmin,
        ["HR Manager"] = HRManager,
        ["HR Specialist"] = HRSpecialist,
        ["Department Manager"] = DepartmentManager,
        ["Dept Manager"] = DepartmentManager,
        ["IT Manager"] = DepartmentManager,
        ["Finance Manager"] = DepartmentManager,
        ["Operations Manager"] = DepartmentManager
    };

    /// <summary>
    /// Checks if a role name is valid
    /// </summary>
    public static bool IsValid(string roleName)
    {
        return All.Contains(roleName, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Converts role aliases to their canonical system role names.
    /// </summary>
    public static string Normalize(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            return roleName;
        }

        var trimmed = roleName.Trim();
        if (AliasToCanonical.TryGetValue(trimmed, out var canonical))
        {
            return canonical;
        }

        // Treat unknown "<Department> Manager" labels as DepartmentManager.
        if (trimmed.EndsWith(" Manager", StringComparison.OrdinalIgnoreCase) ||
            trimmed.EndsWith("Manager", StringComparison.OrdinalIgnoreCase))
        {
            return DepartmentManager;
        }

        var known = All.FirstOrDefault(r => string.Equals(r, trimmed, StringComparison.OrdinalIgnoreCase));
        return known ?? trimmed;
    }

    /// <summary>
    /// Determines whether the supplied role requires a branch scope assignment.
    /// </summary>
    public static bool RequiresBranchScope(string roleName)
    {
        var normalized = Normalize(roleName);
        return BranchScoped.Contains(normalized, StringComparer.OrdinalIgnoreCase);
    }
}