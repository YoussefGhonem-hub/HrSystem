using FluentValidation;

namespace HrSystem.Application.Features.Lifecycle.OffboardingTasks.Queries.GetOffboardingTasksList;

public class GetOffboardingTasksListQueryValidator : AbstractValidator<GetOffboardingTasksListQuery>
{
    public GetOffboardingTasksListQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100");

        RuleFor(x => x.DueDateTo)
            .GreaterThanOrEqualTo(x => x.DueDateFrom)
            .WithMessage("Due date to must be greater than or equal to due date from")
            .When(x => x.DueDateFrom.HasValue && x.DueDateTo.HasValue);
    }
}
