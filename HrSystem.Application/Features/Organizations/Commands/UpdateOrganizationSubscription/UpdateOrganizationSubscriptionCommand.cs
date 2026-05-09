using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Organizations.Commands.UpdateOrganizationSubscription;

public record UpdateOrganizationSubscriptionCommand(
    Guid OrganizationId,
    Guid SubscriptionPlanId,
    DateTime? SubscriptionStartDate,
    DateTime? SubscriptionEndDate,
    string BillingCycle
) : IRequest<ErrorOr<GenericResponse<UpdateOrganizationSubscriptionDto>>>;

public record UpdateOrganizationSubscriptionDto
{
    public Guid OrganizationId { get; init; }
    public Guid SubscriptionPlanId { get; init; }
    public string SubscriptionPlanName { get; init; } = string.Empty;
    public string BillingCycle { get; init; } = "monthly";
    public int ActiveEmployees { get; init; }
    public int MaxEmployees { get; init; }
    public int RemainingSeats { get; init; }
    public bool IsWithinPlanLimit { get; init; }
    public decimal CurrentCyclePrice { get; init; }
    public decimal MonthlyPrice { get; init; }
    public decimal AnnualPrice { get; init; }
    public string Currency { get; init; } = "EGP";
    public DateTime? SubscriptionStartDate { get; init; }
    public DateTime? SubscriptionEndDate { get; init; }
}

public class UpdateOrganizationSubscriptionCommandHandler
    : IRequestHandler<UpdateOrganizationSubscriptionCommand, ErrorOr<GenericResponse<UpdateOrganizationSubscriptionDto>>>
{
    private readonly ApplicationDbContext _context;

    public UpdateOrganizationSubscriptionCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<UpdateOrganizationSubscriptionDto>>> Handle(
        UpdateOrganizationSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        var normalizedBillingCycle = NormalizeBillingCycle(request.BillingCycle);
        if (normalizedBillingCycle is null)
        {
            return Error.Validation(
                code: "Organization.Subscription.BillingCycleInvalid",
                description: "Billing cycle must be either 'monthly' or 'annual'.");
        }

        var organization = await _context.Organizations
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(o => o.Id == request.OrganizationId && !o.IsDeleted, cancellationToken);

        if (organization is null)
        {
            return Error.NotFound(
                code: "Organization.NotFound",
                description: "Organization not found.");
        }

        var plan = await _context.SubscriptionPlans
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == request.SubscriptionPlanId && !p.IsDeleted && p.IsActive, cancellationToken);

        if (plan is null)
        {
            return Error.NotFound(
                code: "SubscriptionPlan.NotFound",
                description: "Subscription plan not found or inactive.");
        }

        var activeEmployees = await _context.Employees
            .IgnoreQueryFilters()
            .CountAsync(
                e => !e.IsDeleted && e.TenantId == organization.Id && e.StatusId != EmployeeStatusIds.Terminated,
                cancellationToken);

        if (activeEmployees > plan.MaxEmployees)
        {
            var exceededBy = activeEmployees - plan.MaxEmployees;
            return Error.Validation(
                code: "Organization.Subscription.LimitExceeded",
                description: $"Organization has {activeEmployees} active employees, but selected plan allows only {plan.MaxEmployees}. Exceeded by {exceededBy} users. Choose a higher plan or reduce active employees.");
        }

        organization.SubscriptionPlanId = plan.Id;
        organization.SubscriptionStartDate = request.SubscriptionStartDate ?? organization.SubscriptionStartDate;
        organization.SubscriptionEndDate = request.SubscriptionEndDate;
        organization.CurrentEmployeeCount = activeEmployees;
        organization.MaxEmployees = plan.MaxEmployees;
        organization.Currency = string.IsNullOrWhiteSpace(organization.Currency) ? plan.Currency : organization.Currency;
        organization.ModifiedDate = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var currentCyclePrice = normalizedBillingCycle == "annual" ? plan.AnnualPrice : plan.MonthlyPrice;
        var remainingSeats = Math.Max(plan.MaxEmployees - activeEmployees, 0);

        var dto = new UpdateOrganizationSubscriptionDto
        {
            OrganizationId = organization.Id,
            SubscriptionPlanId = plan.Id,
            SubscriptionPlanName = string.IsNullOrWhiteSpace(plan.NameEn) ? plan.Code : plan.NameEn,
            BillingCycle = normalizedBillingCycle,
            ActiveEmployees = activeEmployees,
            MaxEmployees = plan.MaxEmployees,
            RemainingSeats = remainingSeats,
            IsWithinPlanLimit = true,
            CurrentCyclePrice = currentCyclePrice,
            MonthlyPrice = plan.MonthlyPrice,
            AnnualPrice = plan.AnnualPrice,
            Currency = string.IsNullOrWhiteSpace(plan.Currency) ? "EGP" : plan.Currency,
            SubscriptionStartDate = organization.SubscriptionStartDate,
            SubscriptionEndDate = organization.SubscriptionEndDate
        };

        return GenericResponse<UpdateOrganizationSubscriptionDto>.SuccessResult(
            dto,
            "Organization subscription updated successfully.");
    }

    private static string? NormalizeBillingCycle(string billingCycle)
    {
        var normalized = (billingCycle ?? string.Empty).Trim().ToLowerInvariant();
        return normalized is "monthly" or "annual" ? normalized : null;
    }
}
