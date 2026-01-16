using FluentValidation;

namespace HrSystem.Application.Features.JobTitles.Commands.UpdateJobTitle;

public class UpdateJobTitleCommandValidator : AbstractValidator<UpdateJobTitleCommand>
{
    public UpdateJobTitleCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Job title ID is required");

        RuleFor(x => x.JobTitle.TitleAr)
            .NotEmpty().WithMessage("Title in Arabic is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.JobTitle.TitleEn)
            .NotEmpty().WithMessage("Title in English is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.JobTitle.Level)
            .GreaterThan(0).WithMessage("Level must be greater than 0");

        RuleFor(x => x.JobTitle.MinSalary)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum salary cannot be negative");

        RuleFor(x => x.JobTitle.MaxSalary)
            .GreaterThan(x => x.JobTitle.MinSalary)
            .WithMessage("Maximum salary must be greater than minimum salary");
    }
}
