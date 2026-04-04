using ErrorOr;
using HrSystem.Application.Features.Performance.GoalMilestones.Common;
using HrSystem.Domain.Entities.Performance;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.GoalMilestones.Commands.CreateGoalMilestone;

public class CreateGoalMilestoneCommandHandler : IRequestHandler<CreateGoalMilestoneCommand, ErrorOr<GenericResponse<GoalMilestoneDto>>>
{
    private readonly ApplicationDbContext _context;

    public CreateGoalMilestoneCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<GoalMilestoneDto>>> Handle(
        CreateGoalMilestoneCommand request,
        CancellationToken cancellationToken)
    {
        var goalMilestone = new GoalMilestone
        {
            GoalId = request.GoalId,
            TitleAr = request.TitleAr,
            TitleEn = request.TitleEn,
            DueDate = request.DueDate,
            IsCompleted = request.IsCompleted,
            CompletionDate = request.IsCompleted ? (request.CompletionDate ?? DateTime.UtcNow) : request.CompletionDate,
            Notes = request.Notes,
            TenantId = Guid.Empty
        };

        _context.GoalMilestones.Add(goalMilestone);
        await _context.SaveChangesAsync(cancellationToken);

        // Reload with navigation properties
        var createdGoalMilestone = await _context.GoalMilestones
            .Include(gm => gm.Goal)
                .ThenInclude(g => g.Employee)
            .FirstAsync(gm => gm.Id == goalMilestone.Id, cancellationToken);

        var dto = new GoalMilestoneDto
        {
            Id = createdGoalMilestone.Id,
            GoalId = createdGoalMilestone.GoalId,
            GoalTitleEn = createdGoalMilestone.Goal.TitleEn,
            GoalTitleAr = createdGoalMilestone.Goal.TitleAr,
            EmployeeName = createdGoalMilestone.Goal.Employee.FullNameEn,
            TitleAr = createdGoalMilestone.TitleAr,
            TitleEn = createdGoalMilestone.TitleEn,
            DueDate = createdGoalMilestone.DueDate,
            IsCompleted = createdGoalMilestone.IsCompleted,
            CompletionDate = createdGoalMilestone.CompletionDate,
            Notes = createdGoalMilestone.Notes,
            CreatedDate = createdGoalMilestone.CreatedDate.DateTime
        };

        return new GenericResponse<GoalMilestoneDto>
        {
            Success = true,
            Data = dto,
            Message = "Goal milestone created successfully"
        };
    }
}
