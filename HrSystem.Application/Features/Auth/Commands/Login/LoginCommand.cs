using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Auth.Commands.Login;

public record LoginCommand(string Username, string Password) : IRequest<ErrorOr<GenericResponse<LoginResponse>>>;

public record LoginResponse(
    string AccessToken,
    DateTime ExpiresAt,
    string UserId,
    string FullName,
    string Email,
    List<string> Roles,
    Guid? BranchId,
    Guid? EmployeeId,
    List<BranchRequestAvailabilityDto> BranchRequestAccess,
    BranchAttendanceConfigDto? BranchAttendanceConfig,
    OrganizationSettingDto? OrganizationSetting);

/// <summary>
/// Lightweight attendance config returned in the login response.
/// Tells the Angular app which check-in methods are available and 
/// provides the check-in points for geofence validation.
/// </summary>
public record BranchAttendanceConfigDto
{
    public Guid SettingId { get; init; }
    public Guid BranchId { get; init; }
    public string PrimaryMethod { get; init; } = "Manual";
    public bool AllowFaceId { get; init; }
    public bool AllowLocation { get; init; }
    public bool AllowFingerprint { get; init; }
    public bool AllowManual { get; init; }
    public bool RequireLocationValidation { get; init; }
    public int DefaultGeofenceRadiusMeters { get; init; } = 200;
    public double FaceIdConfidenceThreshold { get; init; } = 0.85;
    public bool FaceIdRequireLiveness { get; init; } = true;
    public bool AllowMultipleCheckInsPerDay { get; init; }
    public List<CheckInPointInfoDto> CheckInPoints { get; init; } = new();

    /// <summary>
    /// Biometric enrollment status for the logged-in employee.
    /// Null when the user has no linked employee or the branch doesn't use biometrics.
    /// </summary>
    public BiometricEnrollmentDto? BiometricEnrollment { get; init; }
}

/// <summary>
/// Tells the mobile app whether the employee has enrolled their biometric templates.
/// If a required method (FaceId / Fingerprint) is enabled but not enrolled,
/// the app should redirect to the enrollment screen before allowing check-in.
/// </summary>
 public record BiometricEnrollmentDto
{
    /// <summary>True when the employee has an active FaceId template.</summary>
    public bool IsFaceIdEnrolled { get; init; }

    /// <summary>True when the employee has an active Fingerprint template.</summary>
    public bool IsFingerprintEnrolled { get; init; }

    /// <summary>
    /// True when the branch requires a biometric method that the employee has NOT enrolled yet.
    /// The mobile app should show the enrollment screen when this is true.
    /// </summary>
    public bool RequiresEnrollment { get; init; }

    /// <summary>
    /// Which biometric type(s) the employee still needs to enroll.
    /// Empty when fully enrolled or when no biometric method is required.
    /// </summary>
    public List<string> PendingEnrollmentMethods { get; init; } = new();
}

public record CheckInPointInfoDto
{
    public Guid Id { get; init; }
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public int? RadiusMeters { get; init; }
    public bool IsCheckInPoint { get; init; }
    public bool IsCheckOutPoint { get; init; }
    public string? Address { get; init; }
}

/// <summary>
/// Organization settings returned in the login response.
/// Null for SuperAdmin users who are not linked to any organization.
/// </summary>
public record OrganizationSettingDto
{
    public Guid OrganizationId { get; init; }
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string? Industry { get; init; }
    public string? LogoUrl { get; init; }
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Website { get; init; }
    public string? AddressAr { get; init; }
    public string? AddressEn { get; init; }
    public string? City { get; init; }
    public string? Country { get; init; }
    public string TimeZone { get; init; } = "Egypt Standard Time";
    public string Currency { get; init; } = "EGP";
    public string? WeekStartDay { get; init; }
    public string? DefaultLanguage { get; init; } = "en";
    public bool IsActive { get; init; }
    public bool IsTrialPeriod { get; init; }
    public DateTime? TrialEndDate { get; init; }
    public DateTime? SubscriptionEndDate { get; init; }
    public int MaxEmployees { get; init; }
    public int CurrentEmployeeCount { get; init; }
}
