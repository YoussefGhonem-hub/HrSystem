using FluentValidation;

namespace HrSystem.Application.Features.EmployeeRequests.Commands.CreateOvertimeRequest;

public class CreateOvertimeRequestCommandValidator : AbstractValidator<CreateOvertimeRequestCommand>
{
    public CreateOvertimeRequestCommandValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(250);
        RuleFor(x => x.OvertimeDate).NotEmpty();
        RuleFor(x => x.PlannedHours).GreaterThan(TimeSpan.Zero);
        RuleFor(x => x.Multiplier).GreaterThanOrEqualTo(1);

        RuleFor(x => x.ProjectCode).MaximumLength(50);
        RuleFor(x => x.TaskDescription).MaximumLength(500);
    }
}
