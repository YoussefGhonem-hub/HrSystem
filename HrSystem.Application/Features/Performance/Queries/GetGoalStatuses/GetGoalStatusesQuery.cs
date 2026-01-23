using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.Queries.GetGoalStatuses;

public record GetGoalStatusesQuery : IRequest<ErrorOr<GenericResponse<List<GoalStatusDto>>>>;

public class GetGoalStatusesQueryHandler : IRequestHandler<GetGoalStatusesQuery, ErrorOr<GenericResponse<List<GoalStatusDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetGoalStatusesQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<List<GoalStatusDto>>>> Handle(
        GetGoalStatusesQuery request,
        CancellationToken cancellationToken)
    {
        var statuses = await _context.GoalStatuses
            .Where(s => s.IsActive)
            .OrderBy(s => s.DisplayOrder)
            .Select(s => new GoalStatusDto
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

        return new GenericResponse<List<GoalStatusDto>>
        {
            Success = true,
            Data = statuses
        };
    }
}
