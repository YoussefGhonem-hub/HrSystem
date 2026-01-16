using FluentValidation;

namespace HrSystem.Application.Features.Lifecycle.OnboardingTasks.Queries.GetOnboardingTaskById;

public class GetOnboardingTaskByIdQueryValidator : AbstractValidator<GetOnboardingTaskByIdQuery>
{
    public GetOnboardingTaskByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Task ID is required");
    }
}
