using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Organizations.Queries.GetSuperAdminBillingDashboard;

public record GetSuperAdminBillingDashboardQuery
    : IRequest<ErrorOr<GenericResponse<SuperAdminBillingDashboardDto>>>;

public record SuperAdminBillingDashboardDto
{
    public int ActiveOrganizations { get; init; }
    public int NewOrganizationsThisMonth { get; init; }
    public int PayingOrganizations { get; init; }
    public decimal PayingOrganizationsRatioPercent { get; init; }
    public int OpenInvoices { get; init; }
    public int OverdueInvoices { get; init; }
    public decimal MonthlyRevenue { get; init; }
    public decimal MonthlyRevenueChangePercent { get; init; }
    public string Currency { get; init; } = "EGP";
    public DateTime GeneratedAtUtc { get; init; }
}

public class GetSuperAdminBillingDashboardQueryHandler
    : IRequestHandler<GetSuperAdminBillingDashboardQuery, ErrorOr<GenericResponse<SuperAdminBillingDashboardDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetSuperAdminBillingDashboardQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<SuperAdminBillingDashboardDto>>> Handle(
        GetSuperAdminBillingDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var nowUtc = DateTime.UtcNow;
        var currentMonthStart = new DateTime(nowUtc.Year, nowUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var nextMonthStart = currentMonthStart.AddMonths(1);
        var previousMonthStart = currentMonthStart.AddMonths(-1);

        var currentMonthStartOffset = new DateTimeOffset(currentMonthStart, TimeSpan.Zero);
        var nextMonthStartOffset = new DateTimeOffset(nextMonthStart, TimeSpan.Zero);

        var organizationsQuery = _context.Organizations
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(o => !o.IsDeleted);

        var invoicesQuery = _context.OrganizationInvoices
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(i => !i.IsDeleted);

        var activeOrganizations = await organizationsQuery
            .CountAsync(o => o.IsActive, cancellationToken);

        var newOrganizationsThisMonth = await organizationsQuery
            .CountAsync(
                o => o.CreatedDate >= currentMonthStartOffset && o.CreatedDate < nextMonthStartOffset,
                cancellationToken);

        var payingOrganizations = await invoicesQuery
            .Where(i => i.PaidAmount > 0m || i.PaidDate.HasValue || (i.TotalAmount > 0m && i.RemainingAmount <= 0m))
            .Select(i => i.OrganizationId)
            .Distinct()
            .CountAsync(cancellationToken);

        var payingOrganizationsRatio = activeOrganizations <= 0
            ? 0m
            : Math.Round((decimal)payingOrganizations * 100m / activeOrganizations, 1, MidpointRounding.AwayFromZero);

        var openInvoices = await invoicesQuery
            .CountAsync(i => i.TotalAmount > 0m && i.RemainingAmount > 0m, cancellationToken);

        var overdueInvoices = await invoicesQuery
            .CountAsync(
                i => i.TotalAmount > 0m && i.RemainingAmount > 0m && i.DueDate.Date < nowUtc.Date,
                cancellationToken);

        var currentMonthRevenue = await invoicesQuery
            .Where(i => i.PaidDate.HasValue && i.PaidDate.Value >= currentMonthStart && i.PaidDate.Value < nextMonthStart)
            .SumAsync(
                i => (decimal?)(i.PaidAmount > 0m ? i.PaidAmount : i.TotalAmount),
                cancellationToken) ?? 0m;

        var previousMonthRevenue = await invoicesQuery
            .Where(i => i.PaidDate.HasValue && i.PaidDate.Value >= previousMonthStart && i.PaidDate.Value < currentMonthStart)
            .SumAsync(
                i => (decimal?)(i.PaidAmount > 0m ? i.PaidAmount : i.TotalAmount),
                cancellationToken) ?? 0m;

        var monthlyRevenueChangePercent = previousMonthRevenue <= 0m
            ? (currentMonthRevenue > 0m ? 100m : 0m)
            : Math.Round(
                ((currentMonthRevenue - previousMonthRevenue) * 100m) / previousMonthRevenue,
                1,
                MidpointRounding.AwayFromZero);

        var dominantCurrency = await organizationsQuery
            .Where(o => !string.IsNullOrWhiteSpace(o.Currency))
            .GroupBy(o => o.Currency!)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefaultAsync(cancellationToken) ?? "EGP";

        var dto = new SuperAdminBillingDashboardDto
        {
            ActiveOrganizations = activeOrganizations,
            NewOrganizationsThisMonth = newOrganizationsThisMonth,
            PayingOrganizations = payingOrganizations,
            PayingOrganizationsRatioPercent = payingOrganizationsRatio,
            OpenInvoices = openInvoices,
            OverdueInvoices = overdueInvoices,
            MonthlyRevenue = currentMonthRevenue,
            MonthlyRevenueChangePercent = monthlyRevenueChangePercent,
            Currency = dominantCurrency,
            GeneratedAtUtc = nowUtc
        };

        return GenericResponse<SuperAdminBillingDashboardDto>.SuccessResult(
            dto,
            "SuperAdmin billing dashboard retrieved successfully.");
    }
}
