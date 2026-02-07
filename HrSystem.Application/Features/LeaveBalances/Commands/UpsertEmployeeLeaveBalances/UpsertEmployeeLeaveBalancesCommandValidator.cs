using FluentValidation;

namespace HrSystem.Application.Features.LeaveBalances.Commands.UpsertEmployeeLeaveBalances;

public class UpsertEmployeeLeaveBalancesCommandValidator : AbstractValidator<UpsertEmployeeLeaveBalancesCommand>
{
    public UpsertEmployeeLeaveBalancesCommandValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
        RuleFor(x => x.Allocations).NotEmpty();

        RuleForEach(x => x.Allocations)
            .SetValidator(new LeaveBalanceAllocationPayloadValidator());
    }
}

public class LeaveBalanceAllocationPayloadValidator : AbstractValidator<LeaveBalanceAllocationPayload>
{
    public LeaveBalanceAllocationPayloadValidator()
    {
        RuleFor(x => x.VacationTypeId).NotEmpty();
        RuleFor(x => x.AllocatedDays).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CarryOverDays)
            .GreaterThanOrEqualTo(0)
            .When(x => x.CarryOverDays.HasValue);

        RuleFor(x => x.ManualAdjustmentDays)
            .InclusiveBetween(-365, 365)
            .When(x => x.ManualAdjustmentDays.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}
