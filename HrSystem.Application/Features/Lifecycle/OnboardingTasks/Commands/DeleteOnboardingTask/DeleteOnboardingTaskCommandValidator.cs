using FluentValidation;

namespace HrSystem.Application.Features.Lifecycle.OnboardingTasks.Commands.DeleteOnboardingTask;

public class DeleteOnboardingTaskCommandValidator : AbstractValidator<DeleteOnboardingTaskCommand>
{
    public DeleteOnboardingTaskCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Task ID is required");
    }
}
