using FluentValidation;

namespace HrSystem.Application.Features.Lifecycle.OffboardingTasks.Queries.GetOffboardingTaskById;

public class GetOffboardingTaskByIdQueryValidator : AbstractValidator<GetOffboardingTaskByIdQuery>
{
    public GetOffboardingTaskByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Task ID is required");
    }
}
