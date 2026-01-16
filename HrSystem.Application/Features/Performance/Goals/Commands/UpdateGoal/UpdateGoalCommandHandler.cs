using ErrorOr;
using HrSystem.Application.Features.Performance.Goals.Queries.GetGoalById;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.Goals.Commands.UpdateGoal;

public class UpdateGoalCommandHandler : IRequestHandler<UpdateGoalCommand, ErrorOr<GenericResponse<GoalDto>>>
{
    private readonly ApplicationDbContext _context;

    public UpdateGoalCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<GoalDto>>> Handle(
        UpdateGoalCommand request,
        CancellationToken cancellationToken)
    {
        var goal = await _context.Goals
            .FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken);

        if (goal == null)
        {
            return Error.NotFound(description: "Goal not found");
        }

        goal.TitleAr = request.TitleAr;
        goal.TitleEn = request.TitleEn;
        goal.DescriptionAr = request.DescriptionAr;
        goal.DescriptionEn = request.DescriptionEn;
        goal.StartDate = request.StartDate;
        goal.TargetDate = request.TargetDate;
        goal.Status = request.Status;
        goal.Progress = request.Progress;
        goal.Priority = request.Priority;
        goal.AssignedBy = request.AssignedBy;
        goal.CompletionNotes = request.CompletionNotes;

        // Auto-set CompletionDate when status is Completed
        if (request.Status == "Completed" && !goal.CompletionDate.HasValue)
        {
            goal.CompletionDate = DateTime.UtcNow;
        }
        else if (request.Status != "Completed" && goal.CompletionDate.HasValue)
        {
            goal.CompletionDate = null;
        }

        await _context.SaveChangesAsync(cancellationToken);

        var updatedGoal = await _context.Goals
            .Include(g => g.Employee)
            .FirstAsync(g => g.Id == goal.Id, cancellationToken);

        var dto = new GoalDto
        {
            Id = updatedGoal.Id,
            EmployeeId = updatedGoal.EmployeeId,
            EmployeeName = updatedGoal.Employee?.FullNameEn,
            TitleAr = updatedGoal.TitleAr,
            TitleEn = updatedGoal.TitleEn,
            DescriptionAr = updatedGoal.DescriptionAr,
            DescriptionEn = updatedGoal.DescriptionEn,
            StartDate = updatedGoal.StartDate,
            TargetDate = updatedGoal.TargetDate,
            CompletionDate = updatedGoal.CompletionDate,
            Status = updatedGoal.Status,
            Progress = updatedGoal.Progress,
            Priority = updatedGoal.Priority,
            AssignedBy = updatedGoal.AssignedBy,
            CompletionNotes = updatedGoal.CompletionNotes
        };

        return new GenericResponse<GoalDto>
        {
            Success = true,
            Message = "Goal updated successfully",
            Data = dto
        };
    }
}
