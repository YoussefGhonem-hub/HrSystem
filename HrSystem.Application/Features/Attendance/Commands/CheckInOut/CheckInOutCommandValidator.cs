using FluentValidation;
using HrSystem.Domain.Enums;

namespace HrSystem.Application.Features.Attendance.Commands.CheckInOut;

public class CheckInOutCommandValidator : AbstractValidator<CheckInOutCommand>
{
    public CheckInOutCommandValidator()
    {
        RuleFor(x => x.PunchType)
            .IsInEnum().WithMessage("Invalid punch type. Must be CheckIn or CheckOut.");

        RuleFor(x => x.Method)
            .IsInEnum().WithMessage("Invalid attendance method.")
            .Must(m => m != AttendanceMethod.ExcelImport)
            .WithMessage("Use the Excel import endpoint for bulk attendance import.");

        // Location validation
        RuleFor(x => x.Latitude)
            .NotNull().WithMessage("Latitude is required for location-based attendance.")
            .InclusiveBetween(-90.0, 90.0).WithMessage("Latitude must be between -90 and 90.")
            .When(x => x.Method == AttendanceMethod.Location);

        RuleFor(x => x.Longitude)
            .NotNull().WithMessage("Longitude is required for location-based attendance.")
            .InclusiveBetween(-180.0, 180.0).WithMessage("Longitude must be between -180 and 180.")
            .When(x => x.Method == AttendanceMethod.Location);

        // FaceId validation
        RuleFor(x => x.FaceTemplateBase64)
            .NotEmpty().WithMessage("Face template is required for FaceId attendance.")
            .When(x => x.Method == AttendanceMethod.FaceId);
    }
}
