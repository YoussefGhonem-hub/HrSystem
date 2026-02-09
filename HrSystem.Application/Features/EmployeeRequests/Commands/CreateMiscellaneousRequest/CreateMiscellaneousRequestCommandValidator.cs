using FluentValidation;

namespace HrSystem.Application.Features.EmployeeRequests.Commands.CreateMiscellaneousRequest;

public class CreateMiscellaneousRequestCommandValidator : AbstractValidator<CreateMiscellaneousRequestCommand>
{
    public CreateMiscellaneousRequestCommandValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(250);
        RuleFor(x => x.MiscellaneousTypeId).NotEmpty();
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.AdditionalNotes).MaximumLength(2000);
        RuleFor(x => x.ReferenceNumber).MaximumLength(100);
        RuleFor(x => x.Priority).MaximumLength(50);

        When(x => x.StartDate.HasValue && x.EndDate.HasValue, () =>
        {
            RuleFor(x => x)
                .Must(c => c.EndDate!.Value.Date >= c.StartDate!.Value.Date)
                .WithMessage("End date must be greater than or equal to start date.");
        });
    }
}
