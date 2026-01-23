using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Lookups.Queries.GetMaritalStatuses;

/// <summary>
/// Query to get all active marital statuses for dropdown
/// </summary>
public record GetMaritalStatusesQuery : IRequest<ErrorOr<GenericResponse<List<MaritalStatusDto>>>>;

public class GetMaritalStatusesQueryHandler : IRequestHandler<GetMaritalStatusesQuery, ErrorOr<GenericResponse<List<MaritalStatusDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetMaritalStatusesQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<List<MaritalStatusDto>>>> Handle(
        GetMaritalStatusesQuery request,
        CancellationToken cancellationToken)
    {
        var statuses = await _context.MaritalStatuses
            .Where(s => s.IsActive)
            .OrderBy(s => s.DisplayOrder)
            .Select(s => new MaritalStatusDto
            {
                Id = s.Id,
                NameEn = s.NameEn,
                NameAr = s.NameAr,
                DisplayOrder = s.DisplayOrder
            })
            .ToListAsync(cancellationToken);

        return new GenericResponse<List<MaritalStatusDto>>
        {
            Success = true,
            Message = "Marital statuses retrieved successfully",
            Data = statuses
        };
    }
}

public class MaritalStatusDto
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}
