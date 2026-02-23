using HrSystem.Domain.Enums;

namespace HrSystem.Application.Features.Attendance.Dtos;

public record BranchAttendanceSettingDto
{
    public Guid Id { get; init; }
    public Guid BranchId { get; init; }
    public string BranchNameEn { get; init; } = string.Empty;
    public string BranchNameAr { get; init; } = string.Empty;

    // Allowed Methods
    public string PrimaryMethod { get; init; } = string.Empty;
    public bool AllowFaceId { get; init; }
    public bool AllowLocation { get; init; }
    public bool AllowExcelImport { get; init; }
    public bool AllowFingerprint { get; init; }
    public bool AllowManual { get; init; }

    // Location Settings
    public bool RequireLocationValidation { get; init; }
    public int DefaultGeofenceRadiusMeters { get; init; }

    // Auto Checkout
    public bool AutoCheckoutEnabled { get; init; }
    public TimeSpan? AutoCheckoutTime { get; init; }

    // FaceId Settings
    public double FaceIdConfidenceThreshold { get; init; }
    public bool FaceIdRequireLiveness { get; init; }

    // Excel Import
    public bool ExcelImportSkipDuplicates { get; init; }
    public string? ExcelDateFormat { get; init; }

    // General
    public bool AllowMultipleCheckInsPerDay { get; init; }
    public int MinCheckInDurationMinutes { get; init; }
    public string? Notes { get; init; }

    // Check-in Points
    public List<CheckInPointDto> CheckInPoints { get; init; } = new();
}

public record CheckInPointDto
{
    public Guid Id { get; init; }
    public Guid BranchId { get; init; }
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? Description { get; init; }
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public int? RadiusMeters { get; init; }
    public bool IsCheckInPoint { get; init; }
    public bool IsCheckOutPoint { get; init; }
    public bool IsActive { get; init; }
    public string? Address { get; init; }
    public int DisplayOrder { get; init; }
}
