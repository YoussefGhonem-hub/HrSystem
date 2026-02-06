using FluentValidation;

namespace HrSystem.Application.Features.Employees.Commands.SyncEmployeeAssets;

public class SyncEmployeeAssetsCommandValidator : AbstractValidator<SyncEmployeeAssetsCommand>
{
    public SyncEmployeeAssetsCommandValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("Employee ID is required");

        RuleForEach(x => x.Assets).ChildRules(asset =>
        {
            asset.RuleFor(a => a.AssetType)
                .NotEmpty().WithMessage("Asset type is required")
                .MaximumLength(100).WithMessage("Asset type must not exceed 100 characters");

            asset.RuleFor(a => a.AssetName)
                .NotEmpty().WithMessage("Asset name is required")
                .MaximumLength(200).WithMessage("Asset name must not exceed 200 characters");

            asset.RuleFor(a => a.Condition)
                .NotEmpty().WithMessage("Condition is required")
                .MaximumLength(50).WithMessage("Condition must not exceed 50 characters");

            asset.RuleFor(a => a.AssignedDate)
                .NotEmpty().WithMessage("Assigned date is required");

            asset.RuleFor(a => a.ReturnDate)
                .GreaterThanOrEqualTo(a => a.AssignedDate)
                .WithMessage("Return date must be greater than or equal to assigned date")
                .When(a => a.ReturnDate.HasValue);

            asset.RuleFor(a => a.ExpectedReturnDate)
                .GreaterThanOrEqualTo(a => a.AssignedDate)
                .WithMessage("Expected return date must be greater than or equal to assigned date")
                .When(a => a.ExpectedReturnDate.HasValue);

            asset.RuleFor(a => a.Value)
                .GreaterThanOrEqualTo(0).WithMessage("Value must be greater than or equal to 0")
                .When(a => a.Value.HasValue);
        });
    }
}
