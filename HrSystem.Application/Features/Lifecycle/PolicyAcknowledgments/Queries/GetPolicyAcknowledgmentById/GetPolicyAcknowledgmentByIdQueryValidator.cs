using FluentValidation;

namespace HrSystem.Application.Features.Lifecycle.PolicyAcknowledgments.Queries.GetPolicyAcknowledgmentById;

public class GetPolicyAcknowledgmentByIdQueryValidator : AbstractValidator<GetPolicyAcknowledgmentByIdQuery>
{
    public GetPolicyAcknowledgmentByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Policy acknowledgment ID is required");
    }
}
