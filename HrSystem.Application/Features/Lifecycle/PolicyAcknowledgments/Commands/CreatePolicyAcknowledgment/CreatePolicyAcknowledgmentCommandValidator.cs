using FluentValidation;
using HrSystem.Infrustructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Lifecycle.PolicyAcknowledgments.Commands.CreatePolicyAcknowledgment;

public class CreatePolicyAcknowledgmentCommandValidator : AbstractValidator<CreatePolicyAcknowledgmentCommand>
{
    private readonly ApplicationDbContext _context;

    public CreatePolicyAcknowledgmentCommandValidator(ApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("Employee ID is required")
            .MustAsync(EmployeeExists).WithMessage("Employee does not exist");

        RuleFor(x => x.PolicyName)
            .NotEmpty().WithMessage("Policy name is required")
            .MaximumLength(200).WithMessage("Policy name must not exceed 200 characters");

        RuleFor(x => x.PolicyVersion)
            .NotEmpty().WithMessage("Policy version is required")
            .MaximumLength(50).WithMessage("Policy version must not exceed 50 characters");

        RuleFor(x => x.AcknowledgedDate)
            .NotEmpty().WithMessage("Acknowledged date is required");
    }

    private async Task<bool> EmployeeExists(Guid employeeId, CancellationToken cancellationToken)
    {
        return await _context.Employees.AnyAsync(e => e.Id == employeeId, cancellationToken);
    }
}
