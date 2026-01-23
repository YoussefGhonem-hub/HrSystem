using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Lookups.Queries.GetOvertimeStatuses;

public record GetOvertimeStatusesQuery : IRequest<ErrorOr<GenericResponse<List<OvertimeStatusDto>>>>;

public class GetOvertimeStatusesQueryHandler : IRequestHandler<GetOvertimeStatusesQuery, ErrorOr<GenericResponse<List<OvertimeStatusDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetOvertimeStatusesQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<List<OvertimeStatusDto>>>> Handle(
        GetOvertimeStatusesQuery request,
        CancellationToken cancellationToken)
    {
        var statuses = await _context.OvertimeStatuses
            .Where(s => s.IsActive)
            .OrderBy(s => s.DisplayOrder)
            .Select(s => new OvertimeStatusDto
            {
                Id = s.Id,
                NameEn = s.NameEn,
                NameAr = s.NameAr,
                DescriptionEn = s.DescriptionEn,
                DescriptionAr = s.DescriptionAr,
                ColorCode = s.ColorCode,
                DisplayOrder = s.DisplayOrder
            })
            .ToListAsync(cancellationToken);

        return new GenericResponse<List<OvertimeStatusDto>>
        {
            Success = true,
            Data = statuses
        };
    }
}
