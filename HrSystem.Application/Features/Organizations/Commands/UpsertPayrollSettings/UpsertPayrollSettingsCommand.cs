using ErrorOr;
using HrSystem.Application.Features.Organizations.Queries.GetPayrollSettings;
using HrSystem.Domain.Entities.Organization;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Organizations.Commands.UpsertPayrollSettings;

/// <summary>
/// Creates or updates the payroll cycle configuration for the given organization.
/// </summary>
public record UpsertPayrollSettingsCommand(
    Guid OrganizationId,
    PayCycleType CycleType,
    int? CustomCutoffStartDay,
    int? CustomCutoffEndDay,
    DateOnly? AnchorDate,
    string? Notes
) : IRequest<ErrorOr<GenericResponse<PayrollSettingsDto>>>;

public class UpsertPayrollSettingsCommandHandler
    : IRequestHandler<UpsertPayrollSettingsCommand, ErrorOr<GenericResponse<PayrollSettingsDto>>>
{
    private readonly ApplicationDbContext _context;

    public UpsertPayrollSettingsCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<PayrollSettingsDto>>> Handle(
        UpsertPayrollSettingsCommand request,
        CancellationToken cancellationToken)
    {
        if (request.CycleType == PayCycleType.MonthlyCustomCutoff)
        {
            if (request.CustomCutoffStartDay is null or < 1 or > 28)
                return Error.Validation("PayrollSettings.InvalidCutoffStartDay",
                    "Custom cutoff start day must be between 1 and 28.");

            if (request.CustomCutoffEndDay is null or < 1 or > 28)
                return Error.Validation("PayrollSettings.InvalidCutoffEndDay",
                    "Custom cutoff end day must be between 1 and 28.");
        }

        var existing = await _context.OrganizationPayrollSettings
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.TenantId == request.OrganizationId && !s.IsDeleted, cancellationToken);

        if (existing is null)
        {
            existing = new OrganizationPayrollSettings
            {
                TenantId = request.OrganizationId,
                CreatedBy = CurrentUser.Id
            };
            _context.OrganizationPayrollSettings.Add(existing);
        }
        else
        {
            existing.ModifiedBy = CurrentUser.Id;
            existing.ModifiedDate = DateTimeOffset.UtcNow;
        }

        existing.CycleType = request.CycleType;
        existing.CustomCutoffStartDay = request.CycleType == PayCycleType.MonthlyCustomCutoff
            ? request.CustomCutoffStartDay : null;
        existing.CustomCutoffEndDay = request.CycleType == PayCycleType.MonthlyCustomCutoff
            ? request.CustomCutoffEndDay : null;
        existing.AnchorDate = request.CycleType is PayCycleType.BiWeekly or PayCycleType.Weekly
            ? request.AnchorDate : null;
        existing.Notes = request.Notes;

        await _context.SaveChangesAsync(cancellationToken);

        var dto = new PayrollSettingsDto
        {
            Id = existing.Id,
            OrganizationId = request.OrganizationId,
            CycleType = existing.CycleType,
            CycleTypeName = existing.CycleType.ToString(),
            CustomCutoffStartDay = existing.CustomCutoffStartDay,
            CustomCutoffEndDay = existing.CustomCutoffEndDay,
            AnchorDate = existing.AnchorDate,
            Notes = existing.Notes,
            PeriodPreviews = GetPayrollSettingsQueryHandler.BuildPreviews(
                existing.CycleType,
                existing.CustomCutoffStartDay,
                existing.CustomCutoffEndDay,
                existing.AnchorDate)
        };

        return new GenericResponse<PayrollSettingsDto>
        {
            Success = true,
            Message = "Payroll settings saved successfully",
            Data = dto
        };
    }
}
