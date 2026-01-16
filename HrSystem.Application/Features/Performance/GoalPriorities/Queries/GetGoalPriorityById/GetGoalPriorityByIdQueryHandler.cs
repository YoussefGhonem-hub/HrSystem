using ErrorOr;
using HrSystem.Application.Features.Performance.GoalPriorities.Common;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.GoalPriorities.Queries.GetGoalPriorityById;

public class GetGoalPriorityByIdQueryHandler : IRequestHandler<GetGoalPriorityByIdQuery, ErrorOr<GenericResponse<GoalPriorityDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetGoalPriorityByIdQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<GoalPriorityDto>>> Handle(
        GetGoalPriorityByIdQuery request,
        CancellationToken cancellationToken)
    {
        var goalPriority = await _context.GoalPriorities
            .Include(gp => gp.Goals)
            .FirstOrDefaultAsync(gp => gp.Id == request.Id, cancellationToken);

        if (goalPriority == null)
        {
            return Error.NotFound(description: "Goal priority not found");
        }

        var dto = new GoalPriorityDto
        {
            Id = goalPriority.Id,
            NameAr = goalPriority.NameAr,
            NameEn = goalPriority.NameEn,
            DescriptionAr = goalPriority.DescriptionAr,
            DescriptionEn = goalPriority.DescriptionEn,
            DisplayOrder = goalPriority.DisplayOrder,
            IsActive = goalPriority.IsActive,
            GoalsCount = goalPriority.Goals.Count,
            CreatedDate = goalPriority.CreatedDate
        };

        return new GenericResponse<GoalPriorityDto>
        {
            Success = true,
            Data = dto
        };
    }
}
