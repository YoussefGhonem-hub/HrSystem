using FluentValidation;

namespace HrSystem.Application.Features.EmployeeRequests.Commands.UpsertBranchRequestSettings;

public class BranchRequestSettingPayloadValidator : AbstractValidator<BranchRequestSettingPayload>
{
    public BranchRequestSettingPayloadValidator()
    {
        RuleFor(x => x.MaxOpenRequests)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaxOpenRequests.HasValue);
    }
}

public class UpsertBranchRequestSettingsCommandValidator : AbstractValidator<UpsertBranchRequestSettingsCommand>
{
    public UpsertBranchRequestSettingsCommandValidator()
    {
        RuleFor(x => x.BranchId)
            .NotEmpty();

        RuleFor(x => x.Settings)
            .NotEmpty();

        RuleForEach(x => x.Settings)
            .SetValidator(new BranchRequestSettingPayloadValidator());
    }
}
