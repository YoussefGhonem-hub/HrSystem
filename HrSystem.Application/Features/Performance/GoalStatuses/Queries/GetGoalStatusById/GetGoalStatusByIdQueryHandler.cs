using ErrorOr;
using HrSystem.Application.Features.Performance.GoalStatuses.Common;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.GoalStatuses.Queries.GetGoalStatusById;

public class GetGoalStatusByIdQueryHandler : IRequestHandler<GetGoalStatusByIdQuery, ErrorOr<GenericResponse<GoalStatusDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetGoalStatusByIdQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<GoalStatusDto>>> Handle(
        GetGoalStatusByIdQuery request,
        CancellationToken cancellationToken)
    {
        var goalStatus = await _context.GoalStatuses
            .Include(gs => gs.Goals)
            .FirstOrDefaultAsync(gs => gs.Id == request.Id, cancellationToken);

        if (goalStatus == null)
        {
            return Error.NotFound(description: "Goal status not found");
        }

        var dto = new GoalStatusDto
        {
            Id = goalStatus.Id,
            NameAr = goalStatus.NameAr,
            NameEn = goalStatus.NameEn,
            DescriptionAr = goalStatus.DescriptionAr,
            DescriptionEn = goalStatus.DescriptionEn,
            DisplayOrder = goalStatus.DisplayOrder,
            IsActive = goalStatus.IsActive,
            GoalsCount = goalStatus.Goals.Count,
            CreatedDate = goalStatus.CreatedDate
        };

        return new GenericResponse<GoalStatusDto>
        {
            Success = true,
            Data = dto
        };
    }
}
