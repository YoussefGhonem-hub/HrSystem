using FluentValidation;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Commands.CreatePermissionRequest;

public class CreatePermissionRequestCommandValidator : AbstractValidator<CreatePermissionRequestCommand>
{
    private static readonly EmployeeRequestStatus[] CountableStatuses =
    {
        EmployeeRequestStatus.Pending,
        EmployeeRequestStatus.ManagerApproved,
        EmployeeRequestStatus.Approved,
        EmployeeRequestStatus.Completed
    };

    private readonly ApplicationDbContext _context;

    public CreatePermissionRequestCommandValidator(ApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(250);
        RuleFor(x => x.PermissionTypeId).NotEmpty();
        RuleFor(x => x.PermissionDate).NotEmpty();
        RuleFor(x => x.TotalHours).GreaterThan(0);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);

        RuleFor(x => x)
            .Must(c => c.FromTime == null || c.ToTime == null || c.ToTime > c.FromTime)
            .WithMessage("ToTime must be greater than FromTime.");

        RuleFor(x => x)
            .MustAsync(NotExceedPermissionLimit)
            .WithMessage("This request would exceed your monthly permission hours limit for this permission type.")
            .When(x => x.EmployeeId != Guid.Empty && x.PermissionTypeId != Guid.Empty && x.TotalHours > 0);
    }

    private async Task<bool> NotExceedPermissionLimit(
        CreatePermissionRequestCommand command,
        CancellationToken cancellationToken)
    {
        // Look up the employee's permission limit for this permission type
        var limit = await _context.EmployeePermissionLimits
            .AsNoTracking()
            .FirstOrDefaultAsync(
                l => l.EmployeeId == command.EmployeeId && l.PermissionTypeId == command.PermissionTypeId,
                cancellationToken);

        // No limit configured — allow the request
        if (limit == null || !limit.MaxHoursPerMonth.HasValue)
            return true;

        // Sum permission hours already used (approved/pending) in the current month
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);

        var usedHours = await _context.PermissionRequestDetails
            .AsNoTracking()
            .Where(pd => pd.PermissionTypeId == command.PermissionTypeId
                         && pd.EmployeeRequest.EmployeeId == command.EmployeeId
                         && pd.PermissionDate >= monthStart
                         && pd.PermissionDate <= monthEnd
                         && CountableStatuses.Contains(pd.EmployeeRequest.Status))
            .SumAsync(pd => pd.TotalHours, cancellationToken);

        return (usedHours + command.TotalHours) <= limit.MaxHoursPerMonth.Value;
    }
}
