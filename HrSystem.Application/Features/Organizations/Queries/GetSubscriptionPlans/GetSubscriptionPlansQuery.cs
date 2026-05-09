using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Organizations.Queries.GetSubscriptionPlans;

public record GetSubscriptionPlansQuery
    : IRequest<ErrorOr<GenericResponse<List<SubscriptionPlanListDto>>>>;

public record SubscriptionPlanListDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string NameAr { get; init; } = string.Empty;
    public decimal MonthlyPrice { get; init; }
    public decimal AnnualPrice { get; init; }
    public string Currency { get; init; } = "EGP";
    public int MaxEmployees { get; init; }
    public bool IsActive { get; init; }
    public int DisplayOrder { get; init; }
}

public class GetSubscriptionPlansQueryHandler
    : IRequestHandler<GetSubscriptionPlansQuery, ErrorOr<GenericResponse<List<SubscriptionPlanListDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetSubscriptionPlansQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<List<SubscriptionPlanListDto>>>> Handle(
        GetSubscriptionPlansQuery request,
        CancellationToken cancellationToken)
    {
        var plans = await _context.SubscriptionPlans
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(p => !p.IsDeleted && p.IsActive)
            .OrderBy(p => p.DisplayOrder)
            .ThenBy(p => p.NameEn)
            .Select(p => new SubscriptionPlanListDto
            {
                Id = p.Id,
                Code = p.Code,
                NameEn = p.NameEn,
                NameAr = p.NameAr,
                MonthlyPrice = p.MonthlyPrice,
                AnnualPrice = p.AnnualPrice,
                Currency = p.Currency,
                MaxEmployees = p.MaxEmployees,
                IsActive = p.IsActive,
                DisplayOrder = p.DisplayOrder
            })
            .ToListAsync(cancellationToken);

        return GenericResponse<List<SubscriptionPlanListDto>>.SuccessResult(
            plans,
            "Subscription plans retrieved successfully.");
    }
}
