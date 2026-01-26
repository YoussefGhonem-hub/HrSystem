using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Organizations.Queries.GetOrganizationDetails;

public record GetOrganizationDetailsQuery(Guid Id) : IRequest<ErrorOr<GenericResponse<OrganizationDetailsDto>>>;

public record OrganizationDetailsDto
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = string.Empty;
    public string NameAr { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string? LogoUrl { get; init; }
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Website { get; init; }
    public string? AddressAr { get; init; }
    public string? AddressEn { get; init; }
    public string? City { get; init; }
    public string? Country { get; init; }
    public string? PostalCode { get; init; }
    public bool IsActive { get; init; }
    public DateTime SubscriptionStartDate { get; init; }
    public DateTime? SubscriptionEndDate { get; init; }
    public bool IsTrialPeriod { get; init; }
    public DateTime? TrialEndDate { get; init; }
    public string TimeZone { get; init; } = string.Empty;
    public string Currency { get; init; } = string.Empty;
    public string? WeekStartDay { get; init; }
    public List<BranchDetailsDto> Branches { get; init; } = new();
}

public record BranchDetailsDto
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = string.Empty;
    public string NameAr { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public bool IsHeadquarter { get; init; }
    public bool IsActive { get; init; }
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public string? City { get; init; }
}

public class GetOrganizationDetailsQueryHandler : IRequestHandler<GetOrganizationDetailsQuery, ErrorOr<GenericResponse<OrganizationDetailsDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetOrganizationDetailsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<OrganizationDetailsDto>>> Handle(
        GetOrganizationDetailsQuery request,
        CancellationToken cancellationToken)
    {
        var org = await _context.Organizations
            .AsNoTracking()
            .Include(o => o.Branches)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        if (org is null)
        {
            return Error.NotFound("Organization.NotFound", "Organization not found");
        }

        var dto = new OrganizationDetailsDto
        {
            Id = org.Id,
            NameEn = org.NameEn,
            NameAr = org.NameAr,
            Code = org.Code,
            LogoUrl = org.LogoUrl,
            Email = org.Email,
            PhoneNumber = org.PhoneNumber,
            Website = org.Website,
            AddressAr = org.AddressAr,
            AddressEn = org.AddressEn,
            City = org.City,
            Country = org.Country,
            PostalCode = org.PostalCode,
            IsActive = org.IsActive,
            SubscriptionStartDate = org.SubscriptionStartDate,
            SubscriptionEndDate = org.SubscriptionEndDate,
            IsTrialPeriod = org.IsTrialPeriod,
            TrialEndDate = org.TrialEndDate,
            TimeZone = org.TimeZone,
            Currency = org.Currency,
            WeekStartDay = org.WeekStartDay ?? string.Empty,
            Branches = org.Branches
                .OrderByDescending(b => b.IsHeadquarter)
                .ThenBy(b => b.NameEn)
                .Select(b => new BranchDetailsDto
                {
                    Id = b.Id,
                    NameEn = b.NameEn,
                    NameAr = b.NameAr,
                    Code = b.Code,
                    IsHeadquarter = b.IsHeadquarter,
                    IsActive = b.IsActive,
                    Email = b.Email,
                    PhoneNumber = b.PhoneNumber,
                    City = b.City
                }).ToList()
        };

        return new GenericResponse<OrganizationDetailsDto>
        {
            Success = true,
            Data = dto
        };
    }
}
