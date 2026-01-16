using FluentValidation;

namespace HrSystem.Application.Features.Lifecycle.EmployeeAssets.Commands.DeleteEmployeeAsset;

public class DeleteEmployeeAssetCommandValidator : AbstractValidator<DeleteEmployeeAssetCommand>
{
    public DeleteEmployeeAssetCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Asset ID is required");
    }
}
