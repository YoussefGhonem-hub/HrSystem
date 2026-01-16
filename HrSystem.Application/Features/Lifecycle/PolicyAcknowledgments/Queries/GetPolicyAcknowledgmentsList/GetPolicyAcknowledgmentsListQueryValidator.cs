using FluentValidation;

namespace HrSystem.Application.Features.Lifecycle.PolicyAcknowledgments.Queries.GetPolicyAcknowledgmentsList;

public class GetPolicyAcknowledgmentsListQueryValidator : AbstractValidator<GetPolicyAcknowledgmentsListQuery>
{
    public GetPolicyAcknowledgmentsListQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100");

        RuleFor(x => x.AcknowledgedDateTo)
            .GreaterThanOrEqualTo(x => x.AcknowledgedDateFrom)
            .WithMessage("Acknowledged date to must be greater than or equal to acknowledged date from")
            .When(x => x.AcknowledgedDateFrom.HasValue && x.AcknowledgedDateTo.HasValue);
    }
}
