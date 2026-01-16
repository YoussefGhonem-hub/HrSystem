using FluentValidation;

namespace HrSystem.Application.Features.Performance.Feedbacks.Commands.UpdateFeedback;

public class UpdateFeedbackCommandValidator : AbstractValidator<UpdateFeedbackCommand>
{
    public UpdateFeedbackCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Feedback ID is required");

        RuleFor(x => x.FeedbackType)
            .NotEmpty().WithMessage("Feedback type is required")
            .Must(BeValidFeedbackType).WithMessage("Invalid feedback type. Valid values: Manager, Peer, Self, Subordinate");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5");

        RuleFor(x => x.Comments)
            .MaximumLength(2000).WithMessage("Comments must not exceed 2000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Comments));
    }

    private bool BeValidFeedbackType(string feedbackType)
    {
        var validTypes = new[] { "Manager", "Peer", "Self", "Subordinate" };
        return validTypes.Contains(feedbackType);
    }
}
