using ErrorOr;
using HrSystem.Application.Features.Performance.GoalMilestones.Common;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.GoalMilestones.Commands.UpdateGoalMilestone;

public class UpdateGoalMilestoneCommandHandler : IRequestHandler<UpdateGoalMilestoneCommand, ErrorOr<GenericResponse<GoalMilestoneDto>>>
{
    private readonly ApplicationDbContext _context;

    public UpdateGoalMilestoneCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<GoalMilestoneDto>>> Handle(
        UpdateGoalMilestoneCommand request,
        CancellationToken cancellationToken)
    {
        var goalMilestone = await _context.GoalMilestones
            .FirstOrDefaultAsync(gm => gm.Id == request.Id, cancellationToken);

        if (goalMilestone == null)
        {
            return Error.NotFound(description: "Goal milestone not found");
        }

        // Auto-set CompletionDate when IsCompleted changes to true
        var wasNotCompleted = !goalMilestone.IsCompleted;
        var nowCompleted = request.IsCompleted;

        goalMilestone.TitleAr = request.TitleAr;
        goalMilestone.TitleEn = request.TitleEn;
        goalMilestone.DueDate = request.DueDate;
        goalMilestone.IsCompleted = request.IsCompleted;
        goalMilestone.CompletionDate = (wasNotCompleted && nowCompleted) 
            ? (request.CompletionDate ?? DateTime.UtcNow) 
            : request.CompletionDate;
        goalMilestone.Notes = request.Notes;

        await _context.SaveChangesAsync(cancellationToken);

        // Reload with navigation properties
        var updatedGoalMilestone = await _context.GoalMilestones
            .Include(gm => gm.Goal)
                .ThenInclude(g => g.Employee)
            .FirstAsync(gm => gm.Id == goalMilestone.Id, cancellationToken);

        var dto = new GoalMilestoneDto
        {
            Id = updatedGoalMilestone.Id,
            GoalId = updatedGoalMilestone.GoalId,
            GoalTitleEn = updatedGoalMilestone.Goal.TitleEn,
            GoalTitleAr = updatedGoalMilestone.Goal.TitleAr,
            EmployeeName = updatedGoalMilestone.Goal.Employee.FullNameEn,
            TitleAr = updatedGoalMilestone.TitleAr,
            TitleEn = updatedGoalMilestone.TitleEn,
            DueDate = updatedGoalMilestone.DueDate,
            IsCompleted = updatedGoalMilestone.IsCompleted,
            CompletionDate = updatedGoalMilestone.CompletionDate,
            Notes = updatedGoalMilestone.Notes,
            CreatedDate = updatedGoalMilestone.CreatedDate.DateTime
        };

        return new GenericResponse<GoalMilestoneDto>
        {
            Success = true,
            Data = dto,
            Message = "Goal milestone updated successfully"
        };
    }
}
