using HrSystem.Shared.Constants;
using HrSystem.Shared.Extensions;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace HrSystem.Shared.CurrentUser;

public static class CurrentUser
{
    // Initialize this once at startup with the registered IHttpContextAccessor.
    public static void Initialize(IHttpContextAccessor accessor) => HttpContextAccessor = accessor;
    public static IHttpContextAccessor? HttpContextAccessor { get; set; }

    /// <summary>
    /// When true, multi-tenant query filters are bypassed (used during background operations like seeding).
    /// </summary>
    public static bool BypassScopeFilters { get; set; }

    // Claim keys
    private const string SubClaim = "sub";
    private const string NameIdClaim = ClaimTypes.NameIdentifier;
    private const string PreferredUsernameClaim = "preferred_username";
    private const string EmailClaim = "email";
    private const string NameClaim = "name";
    private const string RolesArrayClaim = "roles";
    private const string RoleClaim = ClaimTypes.Role;
    private const string AudienceClaim = "aud";
    private const string OrganizationIdClaim = "organization_id"; // Custom claim for multi-tenancy
    public const string SelectedOrganizationHeaderName = "X-Selected-Organization-Id";
    public const string SelectedOrganizationItemKey = "__CurrentUserSelectedOrganizationId__";
    private const string BranchIdClaim = "branch_id"; // Branch claim for multi-branch scoping
    private const string EmployeeIdClaim = "employee_id"; // Employee ID claim
    private const string JobTitleIdClaim = "job_title_id"; // Job Title ID claim
    private const string DirectManagerIdClaim = "direct_manager_id"; // Direct Manager ID claim
    private const string DepartmentClaim = "department"; // Department name claim

    // Read guest id from request header "X-Guest-UserId" and cache per-request
    public static string? GuestId
    {
        get
        {
            var http = HttpContextAccessor?.HttpContext;
            if (http is null) return null;

            const string cacheKey = "__CurrentUserGuestId__";
            if (http.Items.TryGetValue(cacheKey, out var cached) && cached is string s)
                return s;

            var header = http.Request?.Headers["X-Guest-UserId"].FirstOrDefault();
            var guestId = string.IsNullOrWhiteSpace(header) ? null : header!.Trim();

            if (!string.IsNullOrEmpty(guestId))
                http.Items[cacheKey] = guestId;

            return guestId;
        }
        set
        {
            var http = HttpContextAccessor?.HttpContext;
            if (http is null) return;

            const string cacheKey = "__CurrentUserGuestId__";
            if (string.IsNullOrWhiteSpace(value))
            {
                if (http.Items.ContainsKey(cacheKey))
                    http.Items.Remove(cacheKey);
            }
            else
            {
                http.Items[cacheKey] = value.Trim();
            }
        }
    }

    public static Guid? Id
    {
        get
        {
            // Prefer "sub"; fall back to NameIdentifier or custom "Id"
            var raw = GetClaimValue(SubClaim)
                   ?? GetClaimValue(NameIdClaim)
                   ?? GetClaimValue("Id");
            return Guid.TryParse(raw, out var id) ? id : null;
        }
    }
    
    public static Guid? OrganizationId
    {
        get
        {
            var claimOrganizationId = GetClaimOrganizationId();

            if (IsSuperAdmin)
            {
                return SelectedOrganizationId ?? claimOrganizationId;
            }

            return claimOrganizationId;
        }
    }

    public static Guid? SelectedOrganizationId
    {
        get
        {
            var http = HttpContextAccessor?.HttpContext;
            if (http is null) return null;

            if (!http.Items.TryGetValue(SelectedOrganizationItemKey, out var selectedValue))
                return null;

            return selectedValue switch
            {
                Guid guid => guid,
                string raw when Guid.TryParse(raw, out var parsedGuid) => parsedGuid,
                _ => null
            };
        }
    }

    public static Guid? BranchId
    {
        get
        {
            var raw = GetClaimValue(BranchIdClaim) ?? GetClaimValue("BranchId");
            return Guid.TryParse(raw, out var branchId) ? branchId : null;
        }
    }

    public static Guid? EmployeeId
    {
        get
        {
            var raw = GetClaimValue(EmployeeIdClaim);
            return Guid.TryParse(raw, out var empId) ? empId : null;
        }
    }

    public static Guid? JobTitleId
    {
        get
        {
            var raw = GetClaimValue(JobTitleIdClaim);
            return Guid.TryParse(raw, out var jobId) ? jobId : null;
        }
    }

    public static Guid? DirectManagerId
    {
        get
        {
            var raw = GetClaimValue(DirectManagerIdClaim);
            return Guid.TryParse(raw, out var managerId) ? managerId : null;
        }
    }

    public static string? Department => GetClaimValue(DepartmentClaim);

    public static string UserId => GetClaimValue(SubClaim) ?? GetClaimValue(NameIdClaim) ?? GetClaimValue("Id") ?? string.Empty;
    public static string UserName => GetClaimValue(PreferredUsernameClaim) ?? GetClaimValue(ClaimTypes.Name) ?? string.Empty;
    public static string Email => GetClaimValue(EmailClaim) ?? GetClaimValue(ClaimTypes.Email) ?? string.Empty;
    public static string Name => GetClaimValue(NameClaim) ?? UserName;
    public static IReadOnlyList<string> Roles => GetRoles();
    public static bool IsOrganizationAdmin => Roles.Any(r => string.Equals(r, "Admin", StringComparison.OrdinalIgnoreCase) || string.Equals(r, "OrganizationAdmin", StringComparison.OrdinalIgnoreCase));
    public static bool IsSuperAdmin => Roles.Any(r => string.Equals(r, RoleNames.SuperAdmin, StringComparison.OrdinalIgnoreCase));
    public static IReadOnlyList<string> Permissions => GetPermissions();
    public static IReadOnlyList<string> Audiences => GetAudiences();
    public static bool IsAuthenticated => HttpContextAccessor?.HttpContext?.User?.Identity?.IsAuthenticated == true;

    public static void SetSelectedOrganizationId(Guid? organizationId)
    {
        var http = HttpContextAccessor?.HttpContext;
        if (http is null) return;

        if (organizationId.HasValue)
        {
            http.Items[SelectedOrganizationItemKey] = organizationId;
            return;
        }

        if (http.Items.ContainsKey(SelectedOrganizationItemKey))
        {
            http.Items.Remove(SelectedOrganizationItemKey);
        }
    }

    public static string? GetClaimValue(string key)
    {
        var principal = HttpContextAccessor?.HttpContext?.User;
        if (principal?.Identity is { IsAuthenticated: true })
        {
            var value = principal.Claims.FirstOrDefault(c => c.Type == key)?.Value;
            if (!string.IsNullOrEmpty(value)) return value;
        }

        // Fallback to raw JWT if claim not present on principal
        var jwt = GetRawToken();
        return jwt?.Claims.FirstOrDefault(c => c.Type == key)?.Value;
    }

    public static JwtSecurityToken? GetRawToken()
    {
        var http = HttpContextAccessor?.HttpContext;
        if (http is null) return null;

        const string cacheKey = "__CurrentUserRawJwt__";
        if (http.Items.TryGetValue(cacheKey, out var cached) && cached is JwtSecurityToken jt)
            return jt;

        var header = http.Request.Headers.Authorization.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(header)) return null;

        var bearer = header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? header[7..].Trim()
            : header.Trim();

        if (string.IsNullOrEmpty(bearer)) return null;

        var handler = new JwtSecurityTokenHandler();
        if (!handler.CanReadToken(bearer)) return null;

        var token = handler.ReadToken(bearer) as JwtSecurityToken;
        if (token != null) http.Items[cacheKey] = token;
        return token;
    }

    private static List<string> GetRoles()
    {
        if (!IsAuthenticated) return new();

        var principal = HttpContextAccessor!.HttpContext!.User;
        var roles = new List<string>();

        // Role claims on principal
        roles.AddRange(principal.Claims
            .Where(c => c.Type == RoleClaim || c.Type == "role")
            .Select(c => c.Value));

        // Optional roles array claim (JSON)
        var rolesJson = principal.Claims.FirstOrDefault(c => c.Type == RolesArrayClaim)?.Value;
        if (!string.IsNullOrEmpty(rolesJson))
        {
            try
            {
                var arr = rolesJson.Deserialized<List<string>>();
                if (arr is { Count: > 0 }) roles.AddRange(arr);
            }
            catch { /* ignore malformed */ }
        }

        // Fallback to raw token for any additional roles
        var jwt = GetRawToken();
        if (jwt != null)
        {
            roles.AddRange(jwt.Claims
                .Where(c => c.Type == RoleClaim || c.Type == "role")
                .Select(c => c.Value));

            var jwtRolesJson = jwt.Claims.FirstOrDefault(c => c.Type == RolesArrayClaim)?.Value;
            if (!string.IsNullOrEmpty(jwtRolesJson))
            {
                try
                {
                    var arr = jwtRolesJson.Deserialized<List<string>>();
                    if (arr is { Count: > 0 }) roles.AddRange(arr);
                }
                catch { /* ignore malformed */ }
            }
        }

        return roles
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(r => r)
            .ToList();
    }

    private static List<string> GetPermissions()
    {
        if (!IsAuthenticated) return new();
        var principal = HttpContextAccessor!.HttpContext!.User;

        // Customize if you also emit "permissions" in your JWT
        var permissions = principal.Claims
            .Where(c => c.Type == "permissions")
            .Select(c => c.Value)
            .ToList();

        return permissions
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(p => p)
            .ToList();
    }

    private static List<string> GetAudiences()
    {
        if (!IsAuthenticated) return new();
        var principal = HttpContextAccessor!.HttpContext!.User;

        var auds = principal.Claims.Where(c => c.Type == AudienceClaim).Select(c => c.Value).ToList();
        if (auds.Count == 0)
        {
            var jwt = GetRawToken();
            if (jwt != null)
                auds = jwt.Claims.Where(c => c.Type == AudienceClaim).Select(c => c.Value).ToList();
        }

        if (auds.Count == 0) return new();

        // aud may be a single string or a JSON array
        var result = new List<string>();
        foreach (var value in auds)
        {
            var list = value.Deserialized<List<string>>();
            if (list is { Count: > 0 }) result.AddRange(list);
            else result.Add(value);
        }

        return result
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(a => a)
            .ToList();
    }

    // Manual token parsing helper
    private static Guid? GetClaimOrganizationId()
    {
        var raw = GetClaimValue(OrganizationIdClaim) ?? GetClaimValue("OrganizationId");
        return Guid.TryParse(raw, out var orgId) ? orgId : null;
    }

    // Manual token parsing helper
    public static string GetClaimValue(string token, string key)
    {
        if (string.IsNullOrWhiteSpace(token)) return string.Empty;
        var handler = new JwtSecurityTokenHandler();
        if (!handler.CanReadToken(token)) return string.Empty;

        var jwt = handler.ReadToken(token) as JwtSecurityToken;
        return jwt?.Claims.FirstOrDefault(c => c.Type == key)?.Value ?? string.Empty;
    }
}