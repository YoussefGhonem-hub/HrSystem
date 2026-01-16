using FluentValidation;

namespace HrSystem.Application.Features.Branches.Commands.DeleteBranch;

public class DeleteBranchCommandValidator : AbstractValidator<DeleteBranchCommand>
{
    public DeleteBranchCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Branch ID is required");
    }
}
