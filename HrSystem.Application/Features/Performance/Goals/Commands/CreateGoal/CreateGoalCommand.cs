using ErrorOr;
using HrSystem.Application.Features.Performance.Goals.Queries.GetGoalById;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.Goals.Commands.CreateGoal;

public record CreateGoalCommand(
    Guid EmployeeId,
    string TitleAr,
    string TitleEn,
    string? DescriptionAr,
    string? DescriptionEn,
    DateTime StartDate,
    DateTime TargetDate,
    Guid StatusId,
    Guid PriorityId,
    Guid? AssignedBy
) : IRequest<ErrorOr<GenericResponse<GoalDto>>>;

public class CreateGoalCommandHandler : IRequestHandler<CreateGoalCommand, ErrorOr<GenericResponse<GoalDto>>>
{
    private readonly ApplicationDbContext _context;

    public CreateGoalCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<GoalDto>>> Handle(
        CreateGoalCommand request,
        CancellationToken cancellationToken)
    {
        var goal = new Domain.Entities.Performance.Goal
        {
            EmployeeId = request.EmployeeId,
            TitleAr = request.TitleAr,
            TitleEn = request.TitleEn,
            DescriptionAr = request.DescriptionAr,
            DescriptionEn = request.DescriptionEn,
            StartDate = request.StartDate,
            TargetDate = request.TargetDate,
            StatusId = request.StatusId,
            Progress = 0,
            PriorityId = request.PriorityId,
            AssignedBy = request.AssignedBy,
            TenantId = Guid.NewGuid()
        };

        _context.Goals.Add(goal);
        await _context.SaveChangesAsync(cancellationToken);

        var createdGoal = await _context.Goals
            .Include(g => g.Employee)
            .Include(g => g.Status)
            .Include(g => g.Priority)
            .FirstAsync(g => g.Id == goal.Id, cancellationToken);

        var dto = new GoalDto
        {
            Id = createdGoal.Id,
            EmployeeId = createdGoal.EmployeeId,
            EmployeeName = createdGoal.Employee?.FullNameEn,
            TitleAr = createdGoal.TitleAr,
            TitleEn = createdGoal.TitleEn,
            DescriptionAr = createdGoal.DescriptionAr,
            DescriptionEn = createdGoal.DescriptionEn,
            StartDate = createdGoal.StartDate,
            TargetDate = createdGoal.TargetDate,
            CompletionDate = createdGoal.CompletionDate,
            StatusId = createdGoal.StatusId,
            StatusNameEn = createdGoal.Status?.NameEn ?? string.Empty,
            StatusNameAr = createdGoal.Status?.NameAr ?? string.Empty,
            Progress = createdGoal.Progress,
            PriorityId = createdGoal.PriorityId,
            PriorityNameEn = createdGoal.Priority?.NameEn ?? string.Empty,
            PriorityNameAr = createdGoal.Priority?.NameAr ?? string.Empty,
            AssignedBy = createdGoal.AssignedBy,
            CompletionNotes = createdGoal.CompletionNotes
        };

        return new GenericResponse<GoalDto>
        {
            Success = true,
            Message = "Goal created successfully",
            Data = dto
        };
    }
}
