using ErrorOr;
using HrSystem.Application.Features.Performance.GoalPriorities.Common;
using HrSystem.Domain.Entities.Performance;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.GoalPriorities.Commands.CreateGoalPriority;

public record CreateGoalPriorityCommand(
    string NameAr,
    string NameEn,
    string? DescriptionAr,
    string? DescriptionEn,
    int DisplayOrder,
    bool IsActive
) : IRequest<ErrorOr<GenericResponse<GoalPriorityDto>>>;

public class CreateGoalPriorityCommandHandler : IRequestHandler<CreateGoalPriorityCommand, ErrorOr<GenericResponse<GoalPriorityDto>>>
{
    private readonly ApplicationDbContext _context;

    public CreateGoalPriorityCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<GoalPriorityDto>>> Handle(
        CreateGoalPriorityCommand request,
        CancellationToken cancellationToken)
    {
        var goalPriority = new GoalPriority
        {
            NameAr = request.NameAr,
            NameEn = request.NameEn,
            DescriptionAr = request.DescriptionAr,
            DescriptionEn = request.DescriptionEn,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive
        };

        _context.GoalPriorities.Add(goalPriority);
        await _context.SaveChangesAsync(cancellationToken);

        var createdGoalPriority = await _context.GoalPriorities
            .Include(gp => gp.Goals)
            .FirstAsync(gp => gp.Id == goalPriority.Id, cancellationToken);

        var dto = new GoalPriorityDto
        {
            Id = createdGoalPriority.Id,
            NameAr = createdGoalPriority.NameAr,
            NameEn = createdGoalPriority.NameEn,
            DescriptionAr = createdGoalPriority.DescriptionAr,
            DescriptionEn = createdGoalPriority.DescriptionEn,
            DisplayOrder = createdGoalPriority.DisplayOrder,
            IsActive = createdGoalPriority.IsActive,
            GoalsCount = createdGoalPriority.Goals.Count,
            CreatedDate = createdGoalPriority.CreatedDate
        };

        return new GenericResponse<GoalPriorityDto>
        {
            Success = true,
            Data = dto,
            Message = "Goal priority created successfully"
        };
    }
}
