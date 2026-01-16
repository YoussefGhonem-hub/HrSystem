using ErrorOr;
using HrSystem.Application.Features.Performance.GoalMilestones.Common;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.GoalMilestones.Queries.GetGoalMilestoneById;

public class GetGoalMilestoneByIdQueryHandler : IRequestHandler<GetGoalMilestoneByIdQuery, ErrorOr<GenericResponse<GoalMilestoneDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetGoalMilestoneByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<GoalMilestoneDto>>> Handle(
        GetGoalMilestoneByIdQuery request,
        CancellationToken cancellationToken)
    {
        var goalMilestone = await _context.GoalMilestones
            .Include(gm => gm.Goal)
                .ThenInclude(g => g.Employee)
            .FirstOrDefaultAsync(gm => gm.Id == request.Id, cancellationToken);

        if (goalMilestone == null)
        {
            return Error.NotFound(description: "Goal milestone not found");
        }

        var dto = new GoalMilestoneDto
        {
            Id = goalMilestone.Id,
            GoalId = goalMilestone.GoalId,
            GoalTitleEn = goalMilestone.Goal.TitleEn,
            GoalTitleAr = goalMilestone.Goal.TitleAr,
            EmployeeName = goalMilestone.Goal.Employee.FullNameEn,
            TitleAr = goalMilestone.TitleAr,
            TitleEn = goalMilestone.TitleEn,
            DueDate = goalMilestone.DueDate,
            IsCompleted = goalMilestone.IsCompleted,
            CompletionDate = goalMilestone.CompletionDate,
            Notes = goalMilestone.Notes,
            CreatedDate = goalMilestone.CreatedDate.DateTime
        };

        return new GenericResponse<GoalMilestoneDto>
        {
            Success = true,
            Data = dto
        };
    }
}
