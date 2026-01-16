using FluentValidation;

namespace HrSystem.Application.Features.JobTitles.Commands.DeleteJobTitle;

public class DeleteJobTitleCommandValidator : AbstractValidator<DeleteJobTitleCommand>
{
    public DeleteJobTitleCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Job title ID is required");
    }
}
