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
    public string? Industry { get; init; }
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
    public string? DefaultLanguage { get; init; }
    public OrganizationUserDto? AdminUser { get; init; }
    public OrganizationUserDto? HrManagerUser { get; init; }
    public List<BranchDetailsDto> Branches { get; init; } = new();
}

public record OrganizationUserDto
{
    public Guid Id { get; init; }
    public string? FullName { get; init; }
    public string Email { get; init; } = string.Empty;
    public string? UserName { get; init; }
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

        var adminUser = await (from u in _context.Users.AsNoTracking()
                               join ur in _context.UserRoles.AsNoTracking() on u.Id equals ur.UserId
                               join r in _context.Roles.AsNoTracking() on ur.RoleId equals r.Id
                               where u.OrganizationId == request.Id && r.Name == Shared.Constants.RoleNames.OrganizationAdmin
                               orderby u.CreatedDate
                               select new OrganizationUserDto
                               {
                                   Id = u.Id,
                                   FullName = u.FullName,
                                   Email = u.Email ?? string.Empty,
                                   UserName = u.UserName
                               }).FirstOrDefaultAsync(cancellationToken);

        var hrManagerUser = await (from u in _context.Users.AsNoTracking()
                                   join ur in _context.UserRoles.AsNoTracking() on u.Id equals ur.UserId
                                   join r in _context.Roles.AsNoTracking() on ur.RoleId equals r.Id
                                   where u.OrganizationId == request.Id && r.Name == Shared.Constants.RoleNames.HRManager
                                   orderby u.CreatedDate
                                   select new OrganizationUserDto
                                   {
                                       Id = u.Id,
                                       FullName = u.FullName,
                                       Email = u.Email ?? string.Empty,
                                       UserName = u.UserName
                                   }).FirstOrDefaultAsync(cancellationToken);

        var dto = new OrganizationDetailsDto
        {
            Id = org.Id,
            NameEn = org.NameEn,
            NameAr = org.NameAr,
            Code = org.Code,
            Industry = org.Industry,
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
            DefaultLanguage = org.DefaultLanguage,
            AdminUser = adminUser,
            HrManagerUser = hrManagerUser,
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
