using FluentValidation;

namespace HrSystem.Application.Features.Lifecycle.EmployeeAssets.Commands.UpdateEmployeeAsset;

public class UpdateEmployeeAssetCommandValidator : AbstractValidator<UpdateEmployeeAssetCommand>
{
    public UpdateEmployeeAssetCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Asset ID is required");

        RuleFor(x => x.AssetType)
            .NotEmpty().WithMessage("Asset type is required")
            .MaximumLength(100).WithMessage("Asset type must not exceed 100 characters");

        RuleFor(x => x.AssetName)
            .NotEmpty().WithMessage("Asset name is required")
            .MaximumLength(200).WithMessage("Asset name must not exceed 200 characters");

        RuleFor(x => x.Condition)
            .NotEmpty().WithMessage("Condition is required")
            .MaximumLength(50).WithMessage("Condition must not exceed 50 characters");

        RuleFor(x => x.AssignedDate)
            .NotEmpty().WithMessage("Assigned date is required");

        RuleFor(x => x.ReturnDate)
            .GreaterThanOrEqualTo(x => x.AssignedDate)
            .WithMessage("Return date must be greater than or equal to assigned date")
            .When(x => x.ReturnDate.HasValue);

        RuleFor(x => x.Value)
            .GreaterThanOrEqualTo(0).WithMessage("Value must be greater than or equal to 0")
            .When(x => x.Value.HasValue);
    }
}
