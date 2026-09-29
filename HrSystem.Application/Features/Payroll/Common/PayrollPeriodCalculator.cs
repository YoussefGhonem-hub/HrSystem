using System.Globalization;
using HrSystem.Domain.Entities.Organization;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Payroll.Common;

/// <summary>
/// A single pay period resolved from the organization's payroll cycle settings.
/// Every period belongs to exactly one (Month, Year) bucket: the month in which the period ends.
/// </summary>
public sealed record PayPeriod(
    PayCycleType CycleType,
    int Month,
    int Year,
    int Sequence,
    int PeriodsInMonth,
    DateTime StartDate,
    DateTime EndDate)
{
    public int TotalDays => (EndDate.Date - StartDate.Date).Days + 1;

    /// <summary>How many pay periods of this type fit in a year (used to annualize tax).</summary>
    public int PeriodsPerYear => CycleType switch
    {
        PayCycleType.Weekly => 52,
        PayCycleType.BiWeekly => 26,
        _ => 12
    };

    /// <summary>Factor to convert a monthly amount (salary, allowance, loan installment) into this period's amount.</summary>
    public decimal MonthlyAmountFactor => CycleType switch
    {
        PayCycleType.Weekly => 12m / 52m,
        PayCycleType.BiWeekly => 12m / 26m,
        _ => 1m
    };

    public string MonthLabel => new DateTime(Year, Month, 1).ToString("MMMM yyyy", CultureInfo.InvariantCulture);

    /// <summary>Stable, human readable cycle name stored on <see cref="Domain.Entities.Payroll.PayrollCycle"/>.</summary>
    public string CycleName => CycleType switch
    {
        PayCycleType.MonthlyCalendar => MonthLabel,
        PayCycleType.MonthlyCustomCutoff => $"{MonthLabel} ({StartDate:dd/MM} - {EndDate:dd/MM})",
        PayCycleType.BiWeekly => $"{MonthLabel} - Period {Sequence} ({StartDate:dd/MM} - {EndDate:dd/MM})",
        PayCycleType.Weekly => $"{MonthLabel} - Week {Sequence} ({StartDate:dd/MM} - {EndDate:dd/MM})",
        _ => MonthLabel
    };

    public string RangeLabel => $"{StartDate:dd/MM/yyyy} → {EndDate:dd/MM/yyyy}";
}

/// <summary>
/// Resolves pay periods from <see cref="OrganizationPayrollSettings"/>.
///
/// Conventions:
///   MonthlyCalendar      → 1st .. last day of the month.
///   MonthlyCustomCutoff  → when StartDay &gt; EndDay the period runs from StartDay of the previous month
///                          to EndDay of the month (e.g. 26/04 → 25/05 is the "May" period);
///                          otherwise StartDay .. EndDay within the same month.
///   BiWeekly / Weekly    → fixed-length periods anchored on AnchorDate; a period belongs to the month
///                          in which it ends, so a month can contain several periods.
/// </summary>
public static class PayrollPeriodCalculator
{
    public const int DefaultCutoffStartDay = 26;
    public const int DefaultCutoffEndDay = 25;

    public static Task<OrganizationPayrollSettings?> LoadSettingsAsync(
        ApplicationDbContext context,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        return context.OrganizationPayrollSettings
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && !s.IsDeleted, cancellationToken);
    }

    public static IReadOnlyList<PayPeriod> GetPeriodsForMonth(OrganizationPayrollSettings? settings, int month, int year)
    {
        var cycleType = settings?.CycleType ?? PayCycleType.MonthlyCalendar;

        return cycleType switch
        {
            PayCycleType.MonthlyCustomCutoff => new[]
            {
                BuildCustomCutoffPeriod(
                    settings?.CustomCutoffStartDay ?? DefaultCutoffStartDay,
                    settings?.CustomCutoffEndDay ?? DefaultCutoffEndDay,
                    month,
                    year)
            },
            PayCycleType.BiWeekly => BuildRecurringPeriods(PayCycleType.BiWeekly, ResolveAnchor(settings, year), 14, month, year),
            PayCycleType.Weekly => BuildRecurringPeriods(PayCycleType.Weekly, ResolveAnchor(settings, year), 7, month, year),
            _ => new[] { BuildCalendarMonthPeriod(month, year) }
        };
    }

    /// <summary>
    /// Returns the next <paramref name="count"/> pay periods starting with the one that contains <paramref name="from"/>.
    /// </summary>
    public static IReadOnlyList<PayPeriod> GetUpcomingPeriods(OrganizationPayrollSettings? settings, DateTime from, int count)
    {
        var result = new List<PayPeriod>(count);
        var cycleType = settings?.CycleType ?? PayCycleType.MonthlyCalendar;
        var date = from.Date;

        if (cycleType is PayCycleType.Weekly or PayCycleType.BiWeekly)
        {
            var length = cycleType == PayCycleType.Weekly ? 7 : 14;
            var anchor = ResolveAnchor(settings, date.Year);
            var start = AlignToAnchor(anchor, length, date);
            while (start.AddDays(length - 1) < date) start = start.AddDays(length);

            for (var i = 0; i < count; i++)
            {
                var periodStart = start.AddDays(i * length);
                var periodEnd = periodStart.AddDays(length - 1);
                // Resolve sequence/count by asking for the month the period ends in.
                var monthPeriods = GetPeriodsForMonth(settings, periodEnd.Month, periodEnd.Year);
                var match = monthPeriods.FirstOrDefault(p => p.StartDate == periodStart)
                    ?? new PayPeriod(cycleType, periodEnd.Month, periodEnd.Year, 1, 1, periodStart, periodEnd);
                result.Add(match);
            }

            return result;
        }

        // Monthly variants: find the period containing "from", then walk forward month by month.
        var cursor = new DateTime(date.Year, date.Month, 1);
        var first = GetPeriodsForMonth(settings, cursor.Month, cursor.Year)[0];
        if (first.EndDate < date)
        {
            cursor = cursor.AddMonths(1);
        }
        else if (first.StartDate > date)
        {
            cursor = cursor.AddMonths(-1);
        }

        for (var i = 0; i < count; i++)
        {
            var monthCursor = cursor.AddMonths(i);
            result.Add(GetPeriodsForMonth(settings, monthCursor.Month, monthCursor.Year)[0]);
        }

        return result;
    }

    private static PayPeriod BuildCalendarMonthPeriod(int month, int year)
    {
        var start = new DateTime(year, month, 1);
        var end = start.AddMonths(1).AddDays(-1);
        return new PayPeriod(PayCycleType.MonthlyCalendar, month, year, 1, 1, start, end);
    }

    private static PayPeriod BuildCustomCutoffPeriod(int startDay, int endDay, int month, int year)
    {
        startDay = Math.Clamp(startDay, 1, 31);
        endDay = Math.Clamp(endDay, 1, 31);

        var monthStart = new DateTime(year, month, 1);
        DateTime start;
        var end = new DateTime(year, month, Math.Min(endDay, DateTime.DaysInMonth(year, month)));

        if (startDay > endDay)
        {
            var previous = monthStart.AddMonths(-1);
            start = new DateTime(previous.Year, previous.Month, Math.Min(startDay, DateTime.DaysInMonth(previous.Year, previous.Month)));
        }
        else
        {
            start = new DateTime(year, month, Math.Min(startDay, DateTime.DaysInMonth(year, month)));
        }

        return new PayPeriod(PayCycleType.MonthlyCustomCutoff, month, year, 1, 1, start, end);
    }

    private static IReadOnlyList<PayPeriod> BuildRecurringPeriods(PayCycleType cycleType, DateTime anchor, int lengthDays, int month, int year)
    {
        var monthStart = new DateTime(year, month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);

        var start = AlignToAnchor(anchor, lengthDays, monthStart);
        // Skip periods that end before the month starts.
        while (start.AddDays(lengthDays - 1) < monthStart) start = start.AddDays(lengthDays);

        var periods = new List<(DateTime Start, DateTime End)>();
        while (start.AddDays(lengthDays - 1) <= monthEnd)
        {
            periods.Add((start, start.AddDays(lengthDays - 1)));
            start = start.AddDays(lengthDays);
        }

        return periods
            .Select((p, index) => new PayPeriod(cycleType, month, year, index + 1, periods.Count, p.Start, p.End))
            .ToList();
    }

    /// <summary>Returns the latest period start (anchor + k*length) that is on or before <paramref name="date"/>.</summary>
    private static DateTime AlignToAnchor(DateTime anchor, int lengthDays, DateTime date)
    {
        var diffDays = (date.Date - anchor.Date).Days;
        var steps = (int)Math.Floor(diffDays / (double)lengthDays);
        return anchor.Date.AddDays(steps * lengthDays);
    }

    private static DateTime ResolveAnchor(OrganizationPayrollSettings? settings, int fallbackYear)
    {
        return settings?.AnchorDate.HasValue == true
            ? settings.AnchorDate.Value.ToDateTime(TimeOnly.MinValue)
            : new DateTime(fallbackYear, 1, 1);
    }
}
