using Microsoft.AspNetCore.Identity;

namespace HrSystem.Domain.Entities.Account;

public class ApplicationUser : IdentityUser<Guid>
{
    public string? FullName { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; } = true;
}
