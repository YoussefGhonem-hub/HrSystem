using FluentValidation;

namespace HrSystem.Application.Features.EmployeeRequests.Commands.CreateVacationRequest;

public class CreateVacationRequestCommandValidator : AbstractValidator<CreateVacationRequestCommand>
{
    public CreateVacationRequestCommandValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(250);
        RuleFor(x => x.VacationTypeId).NotEmpty();
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate).NotEmpty();
        RuleFor(x => x.TotalDays).GreaterThan(0);

        RuleFor(x => x)
            .Must(c => c.EndDate.Date >= c.StartDate.Date)
            .WithMessage("End date must be >= start date.");

        RuleFor(x => x.EmergencyContactPhone)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.EmergencyContactPhone));

        RuleFor(x => x.EmergencyContactName)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.EmergencyContactName));
    }
}
