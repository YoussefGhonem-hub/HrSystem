namespace HrSystem.Shared.Constants
{
    public static class RoleNames
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string Admin = "Admin";

        // Add more roles as needed

        /// <summary>
        /// Gets all available role names
        /// </summary>
        public static readonly string[] All = new[]
        {
            SuperAdmin,
            Admin,

        };

        /// <summary>
        /// Checks if a role name is valid
        /// </summary>
        public static bool IsValid(string roleName)
        {
            return All.Contains(roleName, StringComparer.OrdinalIgnoreCase);
        }
    }
}