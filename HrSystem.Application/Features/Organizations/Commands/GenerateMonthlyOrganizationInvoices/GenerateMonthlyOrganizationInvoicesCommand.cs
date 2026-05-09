using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Organizations.Commands.GenerateMonthlyOrganizationInvoices;

public record GenerateMonthlyOrganizationInvoicesCommand(
    int? Year,
    int? Month,
    int DueInDays
) : IRequest<ErrorOr<GenericResponse<GenerateMonthlyOrganizationInvoicesDto>>>;

public record GenerateMonthlyOrganizationInvoicesDto
{
    public int Year { get; init; }
    public int Month { get; init; }
    public DateTime PeriodStartDate { get; init; }
    public DateTime PeriodEndDate { get; init; }
    public int EligibleOrganizations { get; init; }
    public int GeneratedInvoices { get; init; }
    public int SkippedAlreadyGenerated { get; init; }
    public int SkippedNoActiveEmployees { get; init; }
    public int SkippedPlanLimitExceeded { get; init; }
    public decimal TotalInvoicedAmount { get; init; }
    public string Currency { get; init; } = "EGP";
    public DateTime GeneratedAtUtc { get; init; }
    public List<string> Notes { get; init; } = new();
}

public class GenerateMonthlyOrganizationInvoicesCommandHandler
    : IRequestHandler<GenerateMonthlyOrganizationInvoicesCommand, ErrorOr<GenericResponse<GenerateMonthlyOrganizationInvoicesDto>>>
{
    private readonly ApplicationDbContext _context;

    public GenerateMonthlyOrganizationInvoicesCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<GenerateMonthlyOrganizationInvoicesDto>>> Handle(
        GenerateMonthlyOrganizationInvoicesCommand request,
        CancellationToken cancellationToken)
    {
        if (request.DueInDays <= 0 || request.DueInDays > 60)
        {
            return Error.Validation(
                code: "Organization.Invoice.DueInDaysInvalid",
                description: "DueInDays must be between 1 and 60.");
        }

        var year = request.Year ?? DateTime.UtcNow.Year;
        var month = request.Month ?? DateTime.UtcNow.Month;

        if (year < 2000 || year > 2100)
        {
            return Error.Validation(
                code: "Organization.Invoice.YearInvalid",
                description: "Year must be between 2000 and 2100.");
        }

        if (month < 1 || month > 12)
        {
            return Error.Validation(
                code: "Organization.Invoice.MonthInvalid",
                description: "Month must be between 1 and 12.");
        }

        var periodStart = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var nextMonthStart = periodStart.AddMonths(1);
        var periodEnd = nextMonthStart.AddDays(-1);

        var pendingStatusId = await _context.InvoiceStatuses
            .IgnoreQueryFilters()
            .Where(x => !x.IsDeleted && x.IsActive)
            .Where(x =>
                x.Id == HrSystem.Shared.Constants.InvoiceStatusIds.Pending ||
                x.Code == "PENDING" ||
                x.NameEn == "Pending")
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (!pendingStatusId.HasValue)
        {
            return Error.NotFound(
                code: "Organization.InvoiceStatus.PendingNotFound",
                description: "Pending invoice status is missing. Please add an active invoice status with code PENDING.");
        }

        var organizations = await _context.Organizations
            .IgnoreQueryFilters()
            .Include(o => o.SubscriptionPlan)
            .Where(o =>
                !o.IsDeleted &&
                o.IsActive &&
                o.SubscriptionPlanId.HasValue &&
                o.SubscriptionPlan != null &&
                o.SubscriptionPlan.IsActive)
            .ToListAsync(cancellationToken);

        if (organizations.Count == 0)
        {
            var emptyDto = new GenerateMonthlyOrganizationInvoicesDto
            {
                Year = year,
                Month = month,
                PeriodStartDate = periodStart,
                PeriodEndDate = periodEnd,
                GeneratedAtUtc = DateTime.UtcNow,
                Notes = new List<string> { "No active organizations with subscription plans were found." }
            };

            return GenericResponse<GenerateMonthlyOrganizationInvoicesDto>.SuccessResult(
                emptyDto,
                "Monthly invoice generation completed.");
        }

        var organizationIds = organizations.Select(o => o.Id).ToList();

        var activeEmployeeCounts = await _context.Employees
            .IgnoreQueryFilters()
            .Where(e =>
                !e.IsDeleted &&
                organizationIds.Contains(e.TenantId) &&
                e.StatusId != EmployeeStatusIds.Terminated)
            .GroupBy(e => e.TenantId)
            .Select(g => new { OrganizationId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var activeEmployeeCountByOrg = activeEmployeeCounts
            .ToDictionary(x => x.OrganizationId, x => x.Count);

        var existingInvoiceOrganizationIds = await _context.OrganizationInvoices
            .IgnoreQueryFilters()
            .Where(i =>
                !i.IsDeleted &&
                organizationIds.Contains(i.OrganizationId) &&
                i.PeriodStartDate >= periodStart &&
                i.PeriodStartDate < nextMonthStart)
            .Select(i => i.OrganizationId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var existingInvoiceOrgSet = existingInvoiceOrganizationIds.ToHashSet();

        var invoicesToCreate = new List<Domain.Entities.Organization.OrganizationInvoice>();
        var nowUtc = DateTime.UtcNow;

        var skippedAlreadyGenerated = 0;
        var skippedNoActiveEmployees = 0;
        var skippedPlanLimitExceeded = 0;
        decimal totalInvoicedAmount = 0m;

        foreach (var organization in organizations)
        {
            if (existingInvoiceOrgSet.Contains(organization.Id))
            {
                skippedAlreadyGenerated++;
                continue;
            }

            var activeEmployees = activeEmployeeCountByOrg.GetValueOrDefault(organization.Id, 0);
            if (activeEmployees <= 0)
            {
                skippedNoActiveEmployees++;
                continue;
            }

            var plan = organization.SubscriptionPlan!;
            if (activeEmployees > plan.MaxEmployees)
            {
                skippedPlanLimitExceeded++;
                continue;
            }

            var unitPrice = plan.MonthlyPrice;
            var quantity = activeEmployees;
            var subTotal = decimal.Round(unitPrice * quantity, 2, MidpointRounding.AwayFromZero);
            var taxAmount = 0m;
            var totalAmount = decimal.Round(subTotal + taxAmount, 2, MidpointRounding.AwayFromZero);

            var invoice = new Domain.Entities.Organization.OrganizationInvoice
            {
                OrganizationId = organization.Id,
                InvoiceNumber = BuildInvoiceNumber(periodStart, organization.Code, organization.Id),
                InvoiceDate = nowUtc,
                DueDate = nowUtc.AddDays(request.DueInDays),
                SubTotal = subTotal,
                TaxAmount = taxAmount,
                TotalAmount = totalAmount,
                PaidAmount = 0m,
                RemainingAmount = totalAmount,
                StatusId = pendingStatusId.Value,
                Notes = $"Auto-generated monthly subscription invoice for {periodStart:MMMM yyyy}.",
                PeriodStartDate = periodStart,
                PeriodEndDate = periodEnd,
                Items =
                {
                    new Domain.Entities.Organization.OrganizationInvoiceItem
                    {
                        DescriptionAr = $"Subscription fee for {periodStart:MMMM yyyy} ({quantity} users)",
                        DescriptionEn = $"Subscription fee for {periodStart:MMMM yyyy} ({quantity} users)",
                        Quantity = quantity,
                        UnitPrice = unitPrice,
                        Amount = subTotal
                    }
                }
            };

            invoicesToCreate.Add(invoice);
            totalInvoicedAmount += totalAmount;
        }

        if (invoicesToCreate.Count > 0)
        {
            await _context.OrganizationInvoices.AddRangeAsync(invoicesToCreate, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var dominantCurrency = organizations
            .Select(o => string.IsNullOrWhiteSpace(o.Currency) ? null : o.Currency)
            .Where(c => c is not null)
            .GroupBy(c => c!)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault() ?? "EGP";

        var notes = new List<string>();
        if (skippedNoActiveEmployees > 0)
        {
            notes.Add($"Skipped {skippedNoActiveEmployees} organization(s) with zero active employees.");
        }

        if (skippedPlanLimitExceeded > 0)
        {
            notes.Add($"Skipped {skippedPlanLimitExceeded} organization(s) because active employees exceeded plan limits.");
        }

        var dto = new GenerateMonthlyOrganizationInvoicesDto
        {
            Year = year,
            Month = month,
            PeriodStartDate = periodStart,
            PeriodEndDate = periodEnd,
            EligibleOrganizations = organizations.Count,
            GeneratedInvoices = invoicesToCreate.Count,
            SkippedAlreadyGenerated = skippedAlreadyGenerated,
            SkippedNoActiveEmployees = skippedNoActiveEmployees,
            SkippedPlanLimitExceeded = skippedPlanLimitExceeded,
            TotalInvoicedAmount = decimal.Round(totalInvoicedAmount, 2, MidpointRounding.AwayFromZero),
            Currency = dominantCurrency,
            GeneratedAtUtc = nowUtc,
            Notes = notes
        };

        return GenericResponse<GenerateMonthlyOrganizationInvoicesDto>.SuccessResult(
            dto,
            "Monthly invoice generation completed.");
    }

    private static string BuildInvoiceNumber(DateTime periodStart, string? organizationCode, Guid organizationId)
    {
        var normalizedCode = NormalizeOrganizationCode(organizationCode);
        var suffix = organizationId.ToString("N")[..6].ToUpperInvariant();
        return $"INV-{periodStart:yyyyMM}-{normalizedCode}-{suffix}";
    }

    private static string NormalizeOrganizationCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return "ORG";
        }

        var chars = code
            .Where(char.IsLetterOrDigit)
            .Take(12)
            .ToArray();

        if (chars.Length == 0)
        {
            return "ORG";
        }

        return new string(chars).ToUpperInvariant();
    }
}
