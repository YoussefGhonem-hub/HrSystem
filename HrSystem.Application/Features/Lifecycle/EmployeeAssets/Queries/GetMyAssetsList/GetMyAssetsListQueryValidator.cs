using FluentValidation;

namespace HrSystem.Application.Features.Lifecycle.EmployeeAssets.Queries.GetMyAssetsList;

public class GetMyAssetsListQueryValidator : AbstractValidator<GetMyAssetsListQuery>
{
    public GetMyAssetsListQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0");
    }
}
