using System.Linq;
using FluentValidation;

namespace HrSystem.Application.Features.EmployeeRequestLimits.Commands.UpsertEmployeeRequestLimits;

public class UpsertEmployeeRequestLimitsCommandValidator : AbstractValidator<UpsertEmployeeRequestLimitsCommand>
{
    public UpsertEmployeeRequestLimitsCommandValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();

        RuleFor(x => x)
            .Must(cmd => (cmd.VacationLimits?.Count > 0) || (cmd.PermissionLimits?.Count > 0))
            .WithMessage("At least one limit payload is required.");

        When(x => x.VacationLimits != null, () =>
        {
            RuleForEach(x => x.VacationLimits!)
                .SetValidator(new VacationLimitPayloadValidator());

            RuleFor(x => x.VacationLimits!)
                .Must(limits => limits
                    .Select(limit => limit.VacationTypeId)
                    .Distinct()
                    .Count() == limits.Count)
                .WithMessage("Duplicate vacation type entries are not allowed.");
        });

        When(x => x.PermissionLimits != null, () =>
        {
            RuleForEach(x => x.PermissionLimits!)
                .SetValidator(new PermissionLimitPayloadValidator());

            RuleFor(x => x.PermissionLimits!)
                .Must(limits => limits
                    .Select(limit => limit.PermissionTypeId)
                    .Distinct()
                    .Count() == limits.Count)
                .WithMessage("Duplicate permission type entries are not allowed.");
        });
    }
}

public class VacationLimitPayloadValidator : AbstractValidator<VacationLimitPayload>
{
    public VacationLimitPayloadValidator()
    {
        RuleFor(x => x.VacationTypeId).NotEmpty();

        RuleFor(x => x.MaxDaysPerYear)
            .GreaterThan(0)
            .When(x => x.MaxDaysPerYear.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}

public class PermissionLimitPayloadValidator : AbstractValidator<PermissionLimitPayload>
{
    public PermissionLimitPayloadValidator()
    {
        RuleFor(x => x.PermissionTypeId).NotEmpty();

        RuleFor(x => x.MaxHoursPerMonth)
            .GreaterThan(0)
            .When(x => x.MaxHoursPerMonth.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}
