using FluentValidation;

namespace HrSystem.Application.Features.Attendance.Commands.CreateCheckInPoint;

public class CreateCheckInPointCommandValidator : AbstractValidator<CreateCheckInPointCommand>
{
    public CreateCheckInPointCommandValidator()
    {
        RuleFor(x => x.BranchId)
            .NotEmpty().WithMessage("Branch ID is required.");

        RuleFor(x => x.NameAr)
            .NotEmpty().WithMessage("Arabic name is required.")
            .MaximumLength(200);

        RuleFor(x => x.NameEn)
            .NotEmpty().WithMessage("English name is required.")
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(500).When(x => x.Description != null);

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90.0, 90.0).WithMessage("Latitude must be between -90 and 90.");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180.0, 180.0).WithMessage("Longitude must be between -180 and 180.");

        RuleFor(x => x.RadiusMeters)
            .GreaterThan(0).WithMessage("Radius must be greater than 0.")
            .LessThanOrEqualTo(50000).WithMessage("Radius cannot exceed 50,000 meters.")
            .When(x => x.RadiusMeters.HasValue);

        RuleFor(x => x.Address)
            .MaximumLength(500).When(x => x.Address != null);

        RuleFor(x => x)
            .Must(x => x.IsCheckInPoint || x.IsCheckOutPoint)
            .WithMessage("Point must be enabled for at least check-in or check-out.");
    }
}
