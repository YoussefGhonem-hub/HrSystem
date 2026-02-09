using FluentValidation;

namespace HrSystem.Application.Features.EmployeeRequests.Commands.CreateOvertimeRequest;

public class CreateOvertimeRequestCommandValidator : AbstractValidator<CreateOvertimeRequestCommand>
{
    public CreateOvertimeRequestCommandValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(250);
        RuleFor(x => x.OvertimeTypeId).NotEmpty();
        RuleFor(x => x.OvertimeDate).NotEmpty();
        RuleFor(x => x.PlannedHours).NotEmpty()
            .Must(h => h > TimeSpan.Zero)
            .WithMessage("Planned hours must be greater than zero.");
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.ProjectCode).MaximumLength(50);
        RuleFor(x => x.TaskDescription).MaximumLength(1000);
    }
}
