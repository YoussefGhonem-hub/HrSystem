using FluentValidation;

namespace HrSystem.Application.Features.Lifecycle.EmployeeAssets.Queries.GetEmployeeAssetsList;

public class GetEmployeeAssetsListQueryValidator : AbstractValidator<GetEmployeeAssetsListQuery>
{
    public GetEmployeeAssetsListQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100");

        RuleFor(x => x.AssignedDateTo)
            .GreaterThanOrEqualTo(x => x.AssignedDateFrom)
            .WithMessage("Assigned date to must be greater than or equal to assigned date from")
            .When(x => x.AssignedDateFrom.HasValue && x.AssignedDateTo.HasValue);
    }
}
