using FluentValidation;

namespace HrSystem.Application.Features.Branches.Queries.GetBranchById;

public class GetBranchByIdQueryValidator : AbstractValidator<GetBranchByIdQuery>
{
    public GetBranchByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Branch ID is required");
    }
}
