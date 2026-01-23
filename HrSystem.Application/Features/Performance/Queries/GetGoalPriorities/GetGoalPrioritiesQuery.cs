using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.Queries.GetGoalPriorities;

public record GetGoalPrioritiesQuery : IRequest<ErrorOr<GenericResponse<List<GoalPriorityDto>>>>;

public class GetGoalPrioritiesQueryHandler : IRequestHandler<GetGoalPrioritiesQuery, ErrorOr<GenericResponse<List<GoalPriorityDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetGoalPrioritiesQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<List<GoalPriorityDto>>>> Handle(
        GetGoalPrioritiesQuery request,
        CancellationToken cancellationToken)
    {
        var priorities = await _context.GoalPriorities
            .Where(p => p.IsActive)
            .OrderBy(p => p.DisplayOrder)
            .Select(p => new GoalPriorityDto
            {
                Id = p.Id,
                NameEn = p.NameEn,
                NameAr = p.NameAr,
                DescriptionEn = p.DescriptionEn,
                DescriptionAr = p.DescriptionAr,
                ColorCode = p.ColorCode,
                DisplayOrder = p.DisplayOrder
            })
            .ToListAsync(cancellationToken);

        return new GenericResponse<List<GoalPriorityDto>>
        {
            Success = true,
            Data = priorities
        };
    }
}
