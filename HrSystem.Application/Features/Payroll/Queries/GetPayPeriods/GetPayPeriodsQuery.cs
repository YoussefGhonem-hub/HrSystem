using ErrorOr;
using HrSystem.Application.Features.Payroll.Common;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Payroll.Queries.GetPayPeriods;

/// <summary>
/// Returns the pay period(s) that make up the given month for the current organization,
/// resolved from its payroll cycle settings. Used by the "Generate Payslips" dialog.
/// </summary>
public record GetPayPeriodsQuery(int Month, int Year)
    : IRequest<ErrorOr<GenericResponse<PayPeriodsDto>>>;

public class PayPeriodsDto
{
    public PayCycleType CycleType { get; set; }
    public string CycleTypeName { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public List<PayPeriodDto> Periods { get; set; } = new();
}

public class PayPeriodDto
{
    public int Sequence { get; set; }
    public string CycleName { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalDays { get; set; }
    /// <summary>True when the period has already started and can be generated.</summary>
    public bool CanGenerate { get; set; }
    /// <summary>Id of the payroll cycle already created for this period, if any.</summary>
    public Guid? PayrollCycleId { get; set; }
    public int PayslipCount { get; set; }
}

public class GetPayPeriodsQueryHandler : IRequestHandler<GetPayPeriodsQuery, ErrorOr<GenericResponse<PayPeriodsDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetPayPeriodsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<PayPeriodsDto>>> Handle(GetPayPeriodsQuery request, CancellationToken cancellationToken)
    {
        if (request.Month < 1 || request.Month > 12)
            return Error.Validation(description: "Month must be between 1 and 12.");
        if (request.Year < 2000 || request.Year > 2100)
            return Error.Validation(description: "Invalid year.");

        var tenantId = CurrentUser.OrganizationId ?? Guid.Empty;
        var settings = await PayrollPeriodCalculator.LoadSettingsAsync(_context, tenantId, cancellationToken);
        var periods = PayrollPeriodCalculator.GetPeriodsForMonth(settings, request.Month, request.Year);

        var existingCycles = await _context.PayrollCycles
            .Where(c => c.Month == request.Month && c.Year == request.Year)
            .Select(c => new
            {
                c.Id,
                c.PeriodStartDate,
                PayslipCount = c.Payslips.Count(p => !p.IsDeleted)
            })
            .ToListAsync(cancellationToken);

        var today = DateTime.UtcNow.Date;
        var dto = new PayPeriodsDto
        {
            CycleType = settings?.CycleType ?? PayCycleType.MonthlyCalendar,
            CycleTypeName = (settings?.CycleType ?? PayCycleType.MonthlyCalendar).ToString(),
            Month = request.Month,
            Year = request.Year,
            Periods = periods.Select(p =>
            {
                var existing = existingCycles.FirstOrDefault(c => c.PeriodStartDate.Date == p.StartDate.Date);
                return new PayPeriodDto
                {
                    Sequence = p.Sequence,
                    CycleName = p.CycleName,
                    Label = p.RangeLabel,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    TotalDays = p.TotalDays,
                    CanGenerate = p.StartDate.Date <= today,
                    PayrollCycleId = existing?.Id,
                    PayslipCount = existing?.PayslipCount ?? 0
                };
            }).ToList()
        };

        return GenericResponse<PayPeriodsDto>.SuccessResult(dto, "Pay periods resolved successfully.");
    }
}
