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
            Status = goal.Status,
            Progress = goal.Progress,
            Priority = goal.Priority,
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
