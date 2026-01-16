using ErrorOr;
using HrSystem.Application.Features.Performance.GoalPriorities.Common;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.GoalPriorities.Commands.UpdateGoalPriority;

public class UpdateGoalPriorityCommandHandler : IRequestHandler<UpdateGoalPriorityCommand, ErrorOr<GenericResponse<GoalPriorityDto>>>
{
    private readonly ApplicationDbContext _context;

    public UpdateGoalPriorityCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<GoalPriorityDto>>> Handle(
        UpdateGoalPriorityCommand request,
        CancellationToken cancellationToken)
    {
        var goalPriority = await _context.GoalPriorities
            .Include(gp => gp.Goals)
            .FirstOrDefaultAsync(gp => gp.Id == request.Id, cancellationToken);

        if (goalPriority == null)
        {
            return Error.NotFound(description: "Goal priority not found");
        }

        goalPriority.NameAr = request.NameAr;
        goalPriority.NameEn = request.NameEn;
        goalPriority.DescriptionAr = request.DescriptionAr;
        goalPriority.DescriptionEn = request.DescriptionEn;
        goalPriority.DisplayOrder = request.DisplayOrder;
        goalPriority.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

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
            Data = dto,
            Message = "Goal priority updated successfully"
        };
    }
}
