using FluentValidation;
using HrSystem.Domain.Enums;

namespace HrSystem.Application.Features.Attendance.Commands.UpsertBranchAttendanceSetting;

public class UpsertBranchAttendanceSettingCommandValidator : AbstractValidator<UpsertBranchAttendanceSettingCommand>
{
    public UpsertBranchAttendanceSettingCommandValidator()
    {
        RuleFor(x => x.BranchId)
            .NotEmpty().WithMessage("Branch ID is required.");

        RuleFor(x => x.PrimaryMethod)
            .IsInEnum().WithMessage("Invalid attendance method.");

        RuleFor(x => x.DefaultGeofenceRadiusMeters)
            .GreaterThan(0).WithMessage("Geofence radius must be greater than 0.")
            .LessThanOrEqualTo(50000).WithMessage("Geofence radius cannot exceed 50,000 meters.");

        RuleFor(x => x.FaceIdConfidenceThreshold)
            .InclusiveBetween(0.0, 1.0).WithMessage("FaceId confidence threshold must be between 0.0 and 1.0.");

        RuleFor(x => x.MinCheckInDurationMinutes)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum check-in duration cannot be negative.");

        RuleFor(x => x.ExcelDateFormat)
            .MaximumLength(50).When(x => x.ExcelDateFormat != null);

        RuleFor(x => x.Notes)
            .MaximumLength(1000).When(x => x.Notes != null);

        // If location is required, at least AllowLocation or AllowFaceId should be enabled
        RuleFor(x => x)
            .Must(x => !x.RequireLocationValidation || x.AllowLocation || x.AllowFaceId)
            .WithMessage("Location validation requires at least Location or FaceId method to be enabled.");

        // AutoCheckoutTime is required when AutoCheckoutEnabled
        RuleFor(x => x.AutoCheckoutTime)
            .NotNull()
            .When(x => x.AutoCheckoutEnabled)
            .WithMessage("Auto checkout time is required when auto checkout is enabled.");
    }
}
