using FluentValidation;

namespace HrSystem.Application.Features.EmployeeRequests.Commands.CreateFeedbackRequest;

public class CreateFeedbackRequestCommandValidator : AbstractValidator<CreateFeedbackRequestCommand>
{
    public CreateFeedbackRequestCommandValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(250);
        RuleFor(x => x.FeedbackTypeId).NotEmpty();
        RuleFor(x => x.FeedbackContent).NotEmpty().MaximumLength(5000);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.TargetDepartment).MaximumLength(200);
        RuleFor(x => x.TargetPerson).MaximumLength(200);
        RuleFor(x => x.SuggestedImprovement).MaximumLength(5000);

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .When(x => x.Rating.HasValue);
    }
}
