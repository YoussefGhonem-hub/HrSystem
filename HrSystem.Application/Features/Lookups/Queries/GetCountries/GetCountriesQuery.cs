using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Lookups.Queries.GetCountries;

/// <summary>
/// Query to get all active countries for dropdown
/// </summary>
public record GetCountriesQuery : IRequest<ErrorOr<GenericResponse<List<CountryDto>>>>;

public class GetCountriesQueryHandler : IRequestHandler<GetCountriesQuery, ErrorOr<GenericResponse<List<CountryDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetCountriesQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<List<CountryDto>>>> Handle(
        GetCountriesQuery request,
        CancellationToken cancellationToken)
    {
        var countries = await _context.Countries
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new CountryDto
            {
                Id = c.Id,
                NameEn = c.NameEn,
                NameAr = c.NameAr,
                Code = c.Code,
                Currency = c.Currency,
                TimeZone = c.TimeZone,
                PhoneCode = c.PhoneCode,
                DisplayOrder = c.DisplayOrder
            })
            .ToListAsync(cancellationToken);

        return new GenericResponse<List<CountryDto>>
        {
            Success = true,
            Message = "Countries retrieved successfully",
            Data = countries
        };
    }
}

public class CountryDto
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Currency { get; set; }
    public string? TimeZone { get; set; }
    public string? PhoneCode { get; set; }
    public int DisplayOrder { get; set; }
}
