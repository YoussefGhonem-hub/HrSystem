using FluentValidation;

namespace HrSystem.Application.Features.Lifecycle.PolicyAcknowledgments.Commands.DeletePolicyAcknowledgment;

public class DeletePolicyAcknowledgmentCommandValidator : AbstractValidator<DeletePolicyAcknowledgmentCommand>
{
    public DeletePolicyAcknowledgmentCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Policy acknowledgment ID is required");
    }
}
