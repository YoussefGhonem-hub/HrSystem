using FluentValidation;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Commands.CreateVacationRequest;

public class CreateVacationRequestCommandValidator : AbstractValidator<CreateVacationRequestCommand>
{
    private static readonly EmployeeRequestStatus[] CountableStatuses =
    {
        EmployeeRequestStatus.Pending,
        EmployeeRequestStatus.ManagerApproved,
        EmployeeRequestStatus.Approved,
        EmployeeRequestStatus.Completed
    };

    private readonly ApplicationDbContext _context;

    public CreateVacationRequestCommandValidator(ApplicationDbContext context)
    {
        _context = context;

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

        RuleFor(x => x)
            .MustAsync(NotExceedVacationLimit)
            .WithMessage("This request would exceed your annual vacation day limit for this vacation type.")
            .When(x => x.EmployeeId != Guid.Empty && x.VacationTypeId != Guid.Empty && x.TotalDays > 0);
    }

    private async Task<bool> NotExceedVacationLimit(
        CreateVacationRequestCommand command,
        CancellationToken cancellationToken)
    {
        // Look up the employee's vacation limit for this vacation type
        var limit = await _context.EmployeeVacationLimits
            .AsNoTracking()
            .FirstOrDefaultAsync(
                l => l.EmployeeId == command.EmployeeId && l.VacationTypeId == command.VacationTypeId,
                cancellationToken);

        // No limit configured — allow the request
        if (limit == null || !limit.MaxDaysPerYear.HasValue)
            return true;

        // Sum vacation days already used (approved/pending) in the current year
        var currentYear = DateTime.UtcNow.Year;
        var yearStart = new DateTime(currentYear, 1, 1);
        var yearEnd = new DateTime(currentYear, 12, 31);

        var usedDays = await _context.VacationRequestDetails
            .AsNoTracking()
            .Where(vd => vd.VacationTypeId == command.VacationTypeId
                         && vd.EmployeeRequest.EmployeeId == command.EmployeeId
                         && vd.EmployeeRequest.StartDate >= yearStart
                         && vd.EmployeeRequest.StartDate <= yearEnd
                         && CountableStatuses.Contains(vd.EmployeeRequest.Status))
            .SumAsync(vd => vd.TotalDays, cancellationToken);

        return (usedDays + command.TotalDays) <= limit.MaxDaysPerYear.Value;
    }
}
