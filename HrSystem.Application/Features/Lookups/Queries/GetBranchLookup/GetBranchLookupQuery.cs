using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Lookups.Queries.GetBranchLookup;

public record GetBranchLookupQuery() : IRequest<ErrorOr<GenericResponse<List<BranchLookupDto>>>>;

public class GetBranchLookupQueryHandler : IRequestHandler<GetBranchLookupQuery, ErrorOr<GenericResponse<List<BranchLookupDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetBranchLookupQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<List<BranchLookupDto>>>> Handle(GetBranchLookupQuery request, CancellationToken cancellationToken)
    {
        var branches = await _context.Branches
            .Where(b => b.IsActive)
            .OrderBy(b => b.NameEn)
            .Select(b => new BranchLookupDto
            {
                Id = b.Id,
                NameEn = b.NameEn,
                NameAr = b.NameAr,
                Code = b.Code
            })
            .ToListAsync(cancellationToken);

        return new GenericResponse<List<BranchLookupDto>>
        {
            Success = true,
            Message = "Branches retrieved successfully",
            Data = branches
        };
    }
}
