using FluentValidation;

namespace HrSystem.Application.Features.Lifecycle.EmployeeAssets.Queries.GetEmployeeAssetById;

public class GetEmployeeAssetByIdQueryValidator : AbstractValidator<GetEmployeeAssetByIdQuery>
{
    public GetEmployeeAssetByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Asset ID is required");
    }
}
