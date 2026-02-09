using FluentValidation;

namespace HrSystem.Application.Features.EmployeeRequests.Commands.CreatePersonalRequest;

public class CreatePersonalRequestCommandValidator : AbstractValidator<CreatePersonalRequestCommand>
{
    public CreatePersonalRequestCommandValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(250);
        RuleFor(x => x.PersonalTypeId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.PreferredContactMethod).MaximumLength(100);
        RuleFor(x => x.AdditionalContactInfo).MaximumLength(500);

        When(x => x.StartDate.HasValue && x.EndDate.HasValue, () =>
        {
            RuleFor(x => x)
                .Must(c => c.EndDate!.Value.Date >= c.StartDate!.Value.Date)
                .WithMessage("End date must be greater than or equal to start date.");
        });
    }
}
