using FluentValidation;

namespace HrSystem.Application.Features.Lifecycle.PolicyAcknowledgments.Commands.UpdatePolicyAcknowledgment;

public class UpdatePolicyAcknowledgmentCommandValidator : AbstractValidator<UpdatePolicyAcknowledgmentCommand>
{
    public UpdatePolicyAcknowledgmentCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Policy acknowledgment ID is required");

        RuleFor(x => x.PolicyName)
            .NotEmpty().WithMessage("Policy name is required")
            .MaximumLength(200).WithMessage("Policy name must not exceed 200 characters");

        RuleFor(x => x.PolicyVersion)
            .NotEmpty().WithMessage("Policy version is required")
            .MaximumLength(50).WithMessage("Policy version must not exceed 50 characters");

        RuleFor(x => x.AcknowledgedDate)
            .NotEmpty().WithMessage("Acknowledged date is required");
    }
}
