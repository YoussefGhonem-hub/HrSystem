using FluentValidation;

namespace HrSystem.Application.Features.Performance.Goals.Queries.GetGoalById;

public class GetGoalByIdQueryValidator : AbstractValidator<GetGoalByIdQuery>
{
    public GetGoalByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Goal ID is required");
    }
}
