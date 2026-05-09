using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Organizations.Queries.GetOrganizationsList;

public record GetOrganizationsListQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null
) : IRequest<ErrorOr<GenericResponse<PagedResult<OrganizationListDto>>>>;

public record OrganizationListDto
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = string.Empty;
    public string NameAr { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string? Industry { get; init; }
    public string? DefaultLanguage { get; init; }
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public bool IsActive { get; init; }
    public int BranchesCount { get; init; }
    public int EmployeesCount { get; init; }
    public bool IsTrialPeriod { get; init; }
    public DateTime? TrialEndDate { get; init; }
    public DateTime SubscriptionStartDate { get; init; }
    public DateTime? SubscriptionEndDate { get; init; }
    public Guid? SubscriptionPlanId { get; init; }
    public string? SubscriptionPlanCode { get; init; }
    public string? SubscriptionPlanName { get; init; }
    public bool AllowPayrollModule { get; init; }
    public bool AllowPerformanceModule { get; init; }
    public bool AllowRecruitmentModule { get; init; }
    public bool AllowCustomReports { get; init; }
    public bool AllowBiometricIntegration { get; init; }
    public bool AllowAPIAccess { get; init; }
}

public class GetOrganizationsListQueryHandler : IRequestHandler<GetOrganizationsListQuery, ErrorOr<GenericResponse<PagedResult<OrganizationListDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetOrganizationsListQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<PagedResult<OrganizationListDto>>>> Handle(
        GetOrganizationsListQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Organizations
            .IgnoreQueryFilters()
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(o =>
                o.NameEn.Contains(term) ||
                o.NameAr.Contains(term) ||
                o.Code.Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(o => o.CreatedDate)
            .Skip(PagedResultExtensions.CalculateSkip(request.PageNumber, request.PageSize))
            .Take(PagedResultExtensions.CalculateTake(request.PageSize))
            .Select(o => new OrganizationListDto
            {
                Id = o.Id,
                NameEn = o.NameEn,
                NameAr = o.NameAr,
                Code = o.Code,
                Industry = o.Industry,
                DefaultLanguage = o.DefaultLanguage,
                Email = o.Email,
                PhoneNumber = o.PhoneNumber,
                IsActive = o.IsActive,
                BranchesCount = o.Branches.Count,
                EmployeesCount = _context.Employees.Count(e => e.TenantId == o.Id && !e.IsDeleted),
                IsTrialPeriod = o.IsTrialPeriod,
                TrialEndDate = o.TrialEndDate,
                SubscriptionStartDate = o.SubscriptionStartDate,
                SubscriptionEndDate = o.SubscriptionEndDate,
                SubscriptionPlanId = o.SubscriptionPlanId,
                SubscriptionPlanCode = o.SubscriptionPlan != null ? o.SubscriptionPlan.Code : null,
                SubscriptionPlanName = o.SubscriptionPlan != null ? o.SubscriptionPlan.NameEn : null,
                AllowPayrollModule = o.SubscriptionPlan == null || o.SubscriptionPlan.AllowPayrollModule,
                AllowPerformanceModule = o.SubscriptionPlan == null || o.SubscriptionPlan.AllowPerformanceModule,
                AllowRecruitmentModule = o.SubscriptionPlan == null || o.SubscriptionPlan.AllowRecruitmentModule,
                AllowCustomReports = o.SubscriptionPlan == null || o.SubscriptionPlan.AllowCustomReports,
                AllowBiometricIntegration = o.SubscriptionPlan == null || o.SubscriptionPlan.AllowBiometricIntegration,
                AllowAPIAccess = o.SubscriptionPlan == null || o.SubscriptionPlan.AllowAPIAccess
            })
            .ToListAsync(cancellationToken);

        items = items
            .Select(item => item with { DefaultLanguage = LanguageDefaults.NormalizeOrDefault(item.DefaultLanguage) })
            .ToList();

        var paged = PagedResult<OrganizationListDto>.Create(items, totalCount, request.PageNumber, request.PageSize);

        return new GenericResponse<PagedResult<OrganizationListDto>>
        {
            Success = true,
            Data = paged
        };
    }
}
