using ErrorOr;
using HrSystem.Application.Features.Payroll.Common;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Organizations.Queries.GetPayrollSettings;

public record GetPayrollSettingsQuery(Guid OrganizationId)
    : IRequest<ErrorOr<GenericResponse<PayrollSettingsDto>>>;

public record PayrollSettingsDto
{
    public Guid? Id { get; init; }
    public Guid OrganizationId { get; init; }
    public PayCycleType CycleType { get; init; }
    public string CycleTypeName { get; init; } = string.Empty;
    public int? CustomCutoffStartDay { get; init; }
    public int? CustomCutoffEndDay { get; init; }
    public DateOnly? AnchorDate { get; init; }
    public string? Notes { get; init; }
    /// <summary>Preview of the next 6 pay periods based on current settings.</summary>
    public List<PayPeriodPreviewDto> PeriodPreviews { get; init; } = new();
}

public record PayPeriodPreviewDto
{
    public int PeriodNumber { get; init; }
    public string Label { get; init; } = string.Empty;
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
}

public class GetPayrollSettingsQueryHandler
    : IRequestHandler<GetPayrollSettingsQuery, ErrorOr<GenericResponse<PayrollSettingsDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetPayrollSettingsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<PayrollSettingsDto>>> Handle(
        GetPayrollSettingsQuery request,
        CancellationToken cancellationToken)
    {
        var settings = await _context.OrganizationPayrollSettings
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.TenantId == request.OrganizationId && !s.IsDeleted, cancellationToken);

        PayrollSettingsDto dto;
        if (settings is null)
        {
            // Return defaults — no saved settings yet
            dto = new PayrollSettingsDto
            {
                OrganizationId = request.OrganizationId,
                CycleType = PayCycleType.MonthlyCalendar,
                CycleTypeName = nameof(PayCycleType.MonthlyCalendar),
                PeriodPreviews = BuildPreviews(PayCycleType.MonthlyCalendar, null, null, null)
            };
        }
        else
        {
            dto = new PayrollSettingsDto
            {
                Id = settings.Id,
                OrganizationId = request.OrganizationId,
                CycleType = settings.CycleType,
                CycleTypeName = settings.CycleType.ToString(),
                CustomCutoffStartDay = settings.CustomCutoffStartDay,
                CustomCutoffEndDay = settings.CustomCutoffEndDay,
                AnchorDate = settings.AnchorDate,
                Notes = settings.Notes,
                PeriodPreviews = BuildPreviews(
                    settings.CycleType,
                    settings.CustomCutoffStartDay,
                    settings.CustomCutoffEndDay,
                    settings.AnchorDate)
            };
        }

        return new GenericResponse<PayrollSettingsDto>
        {
            Success = true,
            Message = "Payroll settings retrieved",
            Data = dto
        };
    }

    /// <summary>
    /// Builds the upcoming pay period previews using the same calculator that payslip generation uses,
    /// so what HR sees in settings is exactly what "Generate Payslips" will produce.
    /// </summary>
    internal static List<PayPeriodPreviewDto> BuildPreviews(
        PayCycleType cycleType,
        int? cutoffStart,
        int? cutoffEnd,
        DateOnly? anchor,
        int count = 6)
    {
        var settings = new Domain.Entities.Organization.OrganizationPayrollSettings
        {
            CycleType = cycleType,
            CustomCutoffStartDay = cutoffStart,
            CustomCutoffEndDay = cutoffEnd,
            AnchorDate = anchor
        };

        return PayrollPeriodCalculator
            .GetUpcomingPeriods(settings, DateTime.UtcNow.Date, count)
            .Select((period, index) => new PayPeriodPreviewDto
            {
                PeriodNumber = index + 1,
                Label = $"{period.CycleName}: {period.RangeLabel}",
                StartDate = DateOnly.FromDateTime(period.StartDate),
                EndDate = DateOnly.FromDateTime(period.EndDate)
            })
            .ToList();
    }
}
