using ErrorOr;
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

    internal static List<PayPeriodPreviewDto> BuildPreviews(
        PayCycleType cycleType,
        int? cutoffStart,
        int? cutoffEnd,
        DateOnly? anchor,
        int count = 6)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var previews = new List<PayPeriodPreviewDto>(count);

        for (int i = 0; i < count; i++)
        {
            DateOnly start, end;

            switch (cycleType)
            {
                case PayCycleType.MonthlyCalendar:
                {
                    var refDate = today.AddMonths(i);
                    start = new DateOnly(refDate.Year, refDate.Month, 1);
                    end = start.AddMonths(1).AddDays(-1);
                    break;
                }

                case PayCycleType.MonthlyCustomCutoff:
                {
                    int startDay = cutoffStart ?? 26;
                    int endDay = cutoffEnd ?? 25;
                    var refDate = today.AddMonths(i);
                    start = new DateOnly(refDate.Year, refDate.Month, Math.Min(startDay, DateTime.DaysInMonth(refDate.Year, refDate.Month)));
                    var endMonth = start.AddMonths(1);
                    end = new DateOnly(endMonth.Year, endMonth.Month, Math.Min(endDay, DateTime.DaysInMonth(endMonth.Year, endMonth.Month)));
                    break;
                }

                case PayCycleType.BiWeekly:
                {
                    var baseDate = anchor ?? new DateOnly(today.Year, 1, 1);
                    // Find the first period start on or after today - i*14
                    var periodStart = baseDate;
                    while (periodStart.AddDays(14) <= today) periodStart = periodStart.AddDays(14);
                    start = periodStart.AddDays(i * 14);
                    end = start.AddDays(13);
                    break;
                }

                case PayCycleType.Weekly:
                {
                    var baseDate = anchor ?? new DateOnly(today.Year, 1, 1);
                    var periodStart = baseDate;
                    while (periodStart.AddDays(7) <= today) periodStart = periodStart.AddDays(7);
                    start = periodStart.AddDays(i * 7);
                    end = start.AddDays(6);
                    break;
                }

                default:
                    goto case PayCycleType.MonthlyCalendar;
            }

            previews.Add(new PayPeriodPreviewDto
            {
                PeriodNumber = i + 1,
                Label = $"{start:dd/MM/yyyy} → {end:dd/MM/yyyy}",
                StartDate = start,
                EndDate = end
            });
        }

        return previews;
    }
}
