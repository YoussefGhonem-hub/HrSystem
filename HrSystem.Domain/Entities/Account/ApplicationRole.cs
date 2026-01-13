using Microsoft.AspNetCore.Identity;

namespace HrSystem.Domain.Entities.Account;

public class ApplicationRole : IdentityRole<Guid>
{
    public string? DisplayName { get; set; }
}
