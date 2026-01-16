using FluentValidation;

namespace HrSystem.Application.Features.JobTitles.Commands.CreateJobTitle;

public class CreateJobTitleCommandValidator : AbstractValidator<CreateJobTitleCommand>
{
    public CreateJobTitleCommandValidator()
    {
        RuleFor(x => x.TitleAr)
            .NotEmpty().WithMessage("Title in Arabic is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.TitleEn)
            .NotEmpty().WithMessage("Title in English is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Level)
            .GreaterThan(0).WithMessage("Level must be greater than 0");

        RuleFor(x => x.MinSalary)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum salary cannot be negative");

        RuleFor(x => x.MaxSalary)
            .GreaterThan(x => x.MinSalary)
            .WithMessage("Maximum salary must be greater than minimum salary");
    }
}
