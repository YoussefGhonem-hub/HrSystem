using FluentValidation;

namespace HrSystem.Application.Features.Lifecycle.OffboardingTasks.Commands.DeleteOffboardingTask;

public class DeleteOffboardingTaskCommandValidator : AbstractValidator<DeleteOffboardingTaskCommand>
{
    public DeleteOffboardingTaskCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Task ID is required");
    }
}
