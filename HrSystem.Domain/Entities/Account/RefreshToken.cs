using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Account;

/// <summary>
/// Manages secure session tokens for extended user authentication.
/// This entity enables seamless user experience by maintaining secure sessions without requiring
/// frequent re-authentication, while ensuring security through token rotation and revocation.
/// Critical for maintaining both user convenience and system security standards.
/// </summary>
public class RefreshToken : BaseAuditableEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser? User { get; set; }

    // Store only hashes in DB
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }

    // Rotation / revocation
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public string? CreatedByIp { get; set; }

    public DateTime? RevokedAtUtc { get; set; }
    public string? RevokedByIp { get; set; }
    public string? ReplacedByTokenHash { get; set; }
    public string? ReasonRevoked { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;
    public bool IsRevoked => RevokedAtUtc.HasValue;
    public bool IsActive => !IsRevoked && !IsExpired;
}