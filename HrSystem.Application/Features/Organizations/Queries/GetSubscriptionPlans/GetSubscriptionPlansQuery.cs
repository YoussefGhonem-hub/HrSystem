using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Organizations.Queries.GetSubscriptionPlans;

public record GetSubscriptionPlansQuery(bool IncludeInactive = false)
    : IRequest<ErrorOr<GenericResponse<List<SubscriptionPlanListDto>>>>;

public record SubscriptionPlanListDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string NameAr { get; init; } = string.Empty;
    public string? DescriptionEn { get; init; }
    public string? DescriptionAr { get; init; }
    public decimal MonthlyPrice { get; init; }
    public decimal AnnualPrice { get; init; }
    public string Currency { get; init; } = "EGP";
    public int MaxEmployees { get; init; }
    public int MaxStorageGB { get; init; }
    public int MaxDepartments { get; init; }
    public bool AllowBiometricIntegration { get; init; }
    public bool AllowPayrollModule { get; init; }
    public bool AllowPerformanceModule { get; init; }
    public bool AllowRecruitmentModule { get; init; }
    public bool AllowCustomReports { get; init; }
    public bool AllowAPIAccess { get; init; }
    public int TrialDays { get; init; }
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
            .Where(p => !p.IsDeleted && (request.IncludeInactive || p.IsActive))
            .OrderBy(p => p.DisplayOrder)
            .ThenBy(p => p.NameEn)
            .Select(p => new SubscriptionPlanListDto
            {
                Id = p.Id,
                Code = p.Code,
                NameEn = p.NameEn,
                NameAr = p.NameAr,
                DescriptionEn = p.DescriptionEn,
                DescriptionAr = p.DescriptionAr,
                MonthlyPrice = p.MonthlyPrice,
                AnnualPrice = p.AnnualPrice,
                Currency = p.Currency,
                MaxEmployees = p.MaxEmployees,
                MaxStorageGB = p.MaxStorageGB,
                MaxDepartments = p.MaxDepartments,
                AllowBiometricIntegration = p.AllowBiometricIntegration,
                AllowPayrollModule = p.AllowPayrollModule,
                AllowPerformanceModule = p.AllowPerformanceModule,
                AllowRecruitmentModule = p.AllowRecruitmentModule,
                AllowCustomReports = p.AllowCustomReports,
                AllowAPIAccess = p.AllowAPIAccess,
                TrialDays = p.TrialDays,
                IsActive = p.IsActive,
                DisplayOrder = p.DisplayOrder
            })
            .ToListAsync(cancellationToken);

        return GenericResponse<List<SubscriptionPlanListDto>>.SuccessResult(
            plans,
            "Subscription plans retrieved successfully.");
    }
}
