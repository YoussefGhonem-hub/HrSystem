using HrSystem.Domain.Common;
using HrSystem.Domain.Entities.Organization;
using HrSystem.Domain.Enums;

namespace HrSystem.Domain.Entities.Attendance;

public class BranchAttendanceSetting : BaseAuditableEntity
{
    public new Guid BranchId { get; set; }

    // ── Allowed Methods ──────────────────────────────────────
    /// <summary>
    /// Primary attendance method for this branch.
    /// </summary>
    public AttendanceMethod PrimaryMethod { get; set; } = AttendanceMethod.Manual;

    /// <summary>
    /// Whether FaceId check-in is enabled.
    /// </summary>
    public bool AllowFaceId { get; set; }

    /// <summary>
    /// Whether location-based (GPS) check-in is enabled.
    /// </summary>
    public bool AllowLocation { get; set; }

    /// <summary>
    /// Whether attendance can be imported via Excel (fingerprint device export).
    /// </summary>
    public bool AllowExcelImport { get; set; }

    /// <summary>
    /// Whether fingerprint device integration is enabled.
    /// </summary>
    public bool AllowFingerprint { get; set; }

    /// <summary>
    /// Whether HR/Admin can manually create attendance records.
    /// </summary>
    public bool AllowManual { get; set; } = true;

    // ── Location Settings ────────────────────────────────────
    /// <summary>
    /// Whether to enforce GPS location validation during check-in/out (applies to FaceId and Location methods).
    /// </summary>
    public bool RequireLocationValidation { get; set; }

    /// <summary>
    /// Default geofence radius in meters for check-in points.
    /// Each CheckInPoint can override this with its own radius.
    /// </summary>
    public int DefaultGeofenceRadiusMeters { get; set; } = 200;

    // ── Auto Checkout Settings ───────────────────────────────
    /// <summary>
    /// Whether to auto-checkout employees who forgot to check out.
    /// </summary>
    public bool AutoCheckoutEnabled { get; set; }

    /// <summary>
    /// Time at which auto-checkout is applied (e.g., end of shift + buffer).
    /// </summary>
    public TimeSpan? AutoCheckoutTime { get; set; }

    // ── FaceId Settings ──────────────────────────────────────
    /// <summary>
    /// Minimum confidence threshold for FaceId verification (0.0 - 1.0).
    /// </summary>
    public double FaceIdConfidenceThreshold { get; set; } = 0.85;

    /// <summary>
    /// Whether to require liveness detection for FaceId.
    /// </summary>
    public bool FaceIdRequireLiveness { get; set; } = true;

    // ── Excel Import Settings ────────────────────────────────
    /// <summary>
    /// Whether duplicate records should be skipped during Excel import.
    /// </summary>
    public bool ExcelImportSkipDuplicates { get; set; } = true;

    /// <summary>
    /// Expected date format in Excel file (e.g., "yyyy-MM-dd", "dd/MM/yyyy").
    /// </summary>
    public string? ExcelDateFormat { get; set; }

    // ── General Settings ─────────────────────────────────────
    /// <summary>
    /// Whether employees are allowed multiple check-ins per day (e.g. shift-based).
    /// </summary>
    public bool AllowMultipleCheckInsPerDay { get; set; }

    /// <summary>
    /// Minimum minutes between check-in and check-out to consider valid.
    /// </summary>
    public int MinCheckInDurationMinutes { get; set; } = 1;

    /// <summary>
    /// Notes or special instructions for this branch's attendance.
    /// </summary>
    public string? Notes { get; set; }

    // ── Navigation Properties ────────────────────────────────
    public virtual Branch Branch { get; set; } = null!;
    public virtual ICollection<BranchCheckInPoint> CheckInPoints { get; set; } = new List<BranchCheckInPoint>();
}
