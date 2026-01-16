using FluentValidation;
using HrSystem.Infrustructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Lifecycle.EmployeeAssets.Commands.CreateEmployeeAsset;

public class CreateEmployeeAssetCommandValidator : AbstractValidator<CreateEmployeeAssetCommand>
{
    private readonly ApplicationDbContext _context;

    public CreateEmployeeAssetCommandValidator(ApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("Employee ID is required")
            .MustAsync(EmployeeExists).WithMessage("Employee does not exist");

        RuleFor(x => x.AssetType)
            .NotEmpty().WithMessage("Asset type is required")
            .MaximumLength(100).WithMessage("Asset type must not exceed 100 characters");

        RuleFor(x => x.AssetName)
            .NotEmpty().WithMessage("Asset name is required")
            .MaximumLength(200).WithMessage("Asset name must not exceed 200 characters");

        RuleFor(x => x.Condition)
            .NotEmpty().WithMessage("Condition is required")
            .MaximumLength(50).WithMessage("Condition must not exceed 50 characters");

        RuleFor(x => x.AssignedDate)
            .NotEmpty().WithMessage("Assigned date is required");

        RuleFor(x => x.Value)
            .GreaterThanOrEqualTo(0).WithMessage("Value must be greater than or equal to 0")
            .When(x => x.Value.HasValue);
    }

    private async Task<bool> EmployeeExists(Guid employeeId, CancellationToken cancellationToken)
    {
        return await _context.Employees.AnyAsync(e => e.Id == employeeId, cancellationToken);
    }
}
