using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Lookups.Queries.GetGenders;

/// <summary>
/// Query to get all active genders for dropdown
/// </summary>
public record GetGendersQuery : IRequest<ErrorOr<GenericResponse<List<GenderDto>>>>;

public class GetGendersQueryHandler : IRequestHandler<GetGendersQuery, ErrorOr<GenericResponse<List<GenderDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetGendersQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<List<GenderDto>>>> Handle(
        GetGendersQuery request,
        CancellationToken cancellationToken)
    {
        var genders = await _context.Genders
            .Where(g => g.IsActive)
            .OrderBy(g => g.DisplayOrder)
            .Select(g => new GenderDto
            {
                Id = g.Id,
                NameEn = g.NameEn,
                NameAr = g.NameAr,
                DisplayOrder = g.DisplayOrder
            })
            .ToListAsync(cancellationToken);

        return new GenericResponse<List<GenderDto>>
        {
            Success = true,
            Message = "Genders retrieved successfully",
            Data = genders
        };
    }
}

public class GenderDto
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}
