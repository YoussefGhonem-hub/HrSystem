using FluentValidation;

namespace HrSystem.Application.Features.EmployeeRequests.Commands.CreateEmployeeRequest;

public class CreateEmployeeRequestCommandValidator : AbstractValidator<CreateEmployeeRequestCommand>
{
    public CreateEmployeeRequestCommandValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(250);

        RuleFor(x => x.Description)
            .MaximumLength(2000);

        When(x => x.StartDate.HasValue && x.EndDate.HasValue, () =>
        {
            RuleFor(x => x)
                .Must(c => c.EndDate!.Value.Date >= c.StartDate!.Value.Date)
                .WithMessage("End date must be greater than or equal to start date.");
        });

        When(x => x.RequestTypeCode == "Vacation", () =>
        {
            RuleFor(x => x.StartDate)
                .NotNull();

            RuleFor(x => x.EndDate)
                .NotNull();
        });

        When(x => x.RequestTypeCode == "OverTime", () =>
        {
            RuleFor(x => x.StartDate)
                .NotNull()
                .WithMessage("Overtime date is required.");
        });

        When(x => x.RequestTypeCode == "Training", () =>
        {
            RuleFor(x => x.Description)
                .NotEmpty()
                .WithMessage("Please provide a short justification for the training request.");
        });
    }
}
