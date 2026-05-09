using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Organizations.Queries.GetOrganizationTrialStatus;

public record GetOrganizationTrialStatusQuery(Guid OrganizationId)
    : IRequest<ErrorOr<GenericResponse<OrganizationTrialStatusDto>>>;

public record OrganizationTrialStatusDto
{
    public Guid OrganizationId { get; init; }
    public bool IsActive { get; init; }
    public bool IsTrialPeriod { get; init; }
    public DateTime? TrialEndDate { get; init; }
    public DateTime? SubscriptionEndDate { get; init; }
    public bool IsTrialPeriodDone { get; init; }
    public bool IsSubscriptionExpired { get; init; }
    public bool ShouldBlockApplication { get; init; }
}

public class GetOrganizationTrialStatusQueryHandler
    : IRequestHandler<GetOrganizationTrialStatusQuery, ErrorOr<GenericResponse<OrganizationTrialStatusDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetOrganizationTrialStatusQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<OrganizationTrialStatusDto>>> Handle(
        GetOrganizationTrialStatusQuery request,
        CancellationToken cancellationToken)
    {
        var org = await _context.Organizations
            .AsNoTracking()
            .Where(o => o.Id == request.OrganizationId)
            .Select(o => new
            {
                o.Id,
                o.IsActive,
                o.IsTrialPeriod,
                o.TrialEndDate,
                o.SubscriptionEndDate
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (org is null)
        {
            return Error.NotFound("Organization.NotFound", "Organization not found");
        }

        var nowUtc = DateTime.UtcNow;
        var isTrialDone = org.IsTrialPeriod && org.TrialEndDate.HasValue && org.TrialEndDate.Value <= nowUtc;
        var isSubscriptionExpired = org.SubscriptionEndDate.HasValue && org.SubscriptionEndDate.Value <= nowUtc;

        var dto = new OrganizationTrialStatusDto
        {
            OrganizationId = org.Id,
            IsActive = org.IsActive,
            IsTrialPeriod = org.IsTrialPeriod,
            TrialEndDate = org.TrialEndDate,
            SubscriptionEndDate = org.SubscriptionEndDate,
            IsTrialPeriodDone = isTrialDone,
            IsSubscriptionExpired = isSubscriptionExpired,
            ShouldBlockApplication = !org.IsActive || isTrialDone || isSubscriptionExpired
        };

        return new GenericResponse<OrganizationTrialStatusDto>
        {
            Success = true,
            Message = "Organization trial status retrieved successfully",
            Data = dto
        };
    }
}
