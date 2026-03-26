using HrSystem.Domain.Common;
using HrSystem.Domain.Entities.Organization;

namespace HrSystem.Domain.Entities.Attendance;

public class BranchCheckInPoint : BaseAuditableEntity
{

    /// <summary>
    /// The attendance setting this check-in point belongs to.
    /// </summary>
    public Guid BranchAttendanceSettingId { get; set; }

    /// <summary>
    /// Name of the check-in point (e.g., "Main Gate", "Back Entrance", "Building A").
    /// </summary>
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;

    /// <summary>
    /// Optional description or instructions for this point.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// GPS latitude of this check-in point.
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    /// GPS longitude of this check-in point.
    /// </summary>
    public double Longitude { get; set; }

    /// <summary>
    /// Geofence radius in meters. If null, uses the branch default radius.
    /// </summary>
    public int? RadiusMeters { get; set; }

    /// <summary>
    /// Whether this point is used for check-in.
    /// </summary>
    public bool IsCheckInPoint { get; set; } = true;

    /// <summary>
    /// Whether this point is used for check-out.
    /// </summary>
    public bool IsCheckOutPoint { get; set; } = true;

    /// <summary>
    /// Whether this check-in point is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Optional address or landmark for display purposes.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Display order for UI.
    /// </summary>
    public int DisplayOrder { get; set; }

    // ── Navigation Properties ────────────────────────────────
    public virtual Branch Branch { get; set; } = null!;
    public virtual BranchAttendanceSetting BranchAttendanceSetting { get; set; } = null!;
}
