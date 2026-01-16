using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.Goals.Queries.GetGoalById;

public class GetGoalByIdQueryHandler : IRequestHandler<GetGoalByIdQuery, ErrorOr<GenericResponse<GoalDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetGoalByIdQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<GoalDto>>> Handle(
        GetGoalByIdQuery request,
        CancellationToken cancellationToken)
    {
        var goal = await _context.Goals
            .Include(g => g.Employee)
            .Include(g => g.Status)
            .Include(g => g.Priority)
            .FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken);

        if (goal == null)
        {
            return Error.NotFound(description: "Goal not found");
        }

        var dto = new GoalDto
        {
            Id = goal.Id,
            EmployeeId = goal.EmployeeId,
            EmployeeName = goal.Employee?.FullNameEn,
            TitleAr = goal.TitleAr,
            TitleEn = goal.TitleEn,
            DescriptionAr = goal.DescriptionAr,
            DescriptionEn = goal.DescriptionEn,
            StartDate = goal.StartDate,
            TargetDate = goal.TargetDate,
            CompletionDate = goal.CompletionDate,
            StatusId = goal.StatusId,
            StatusNameEn = goal.Status?.NameEn ?? string.Empty,
            StatusNameAr = goal.Status?.NameAr ?? string.Empty,
            Progress = goal.Progress,
            PriorityId = goal.PriorityId,
            PriorityNameEn = goal.Priority?.NameEn ?? string.Empty,
            PriorityNameAr = goal.Priority?.NameAr ?? string.Empty,
            AssignedBy = goal.AssignedBy,
            CompletionNotes = goal.CompletionNotes
        };

        return new GenericResponse<GoalDto>
        {
            Success = true,
            Data = dto
        };
    }
}
