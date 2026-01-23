using HrSystem.Domain.Common;
using HrSystem.Domain.Entities.Organization;

namespace HrSystem.Domain.Entities.Account;

/// <summary>
/// Maps a user role to a specific branch scope.
/// </summary>
public class UserBranchRole : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid BranchId { get; set; }
    public string RoleName { get; set; } = string.Empty;

    public virtual ApplicationUser User { get; set; } = null!;
    public virtual Branch Branch { get; set; } = null!;
}
