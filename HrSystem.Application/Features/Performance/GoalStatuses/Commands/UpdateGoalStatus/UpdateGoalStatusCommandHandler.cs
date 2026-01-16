using ErrorOr;
using HrSystem.Application.Features.Performance.GoalStatuses.Common;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.GoalStatuses.Commands.UpdateGoalStatus;

public class UpdateGoalStatusCommandHandler : IRequestHandler<UpdateGoalStatusCommand, ErrorOr<GenericResponse<GoalStatusDto>>>
{
    private readonly ApplicationDbContext _context;

    public UpdateGoalStatusCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<GoalStatusDto>>> Handle(
        UpdateGoalStatusCommand request,
        CancellationToken cancellationToken)
    {
        var goalStatus = await _context.GoalStatuses
            .Include(gs => gs.Goals)
            .FirstOrDefaultAsync(gs => gs.Id == request.Id, cancellationToken);

        if (goalStatus == null)
        {
            return Error.NotFound(description: "Goal status not found");
        }

        goalStatus.NameAr = request.NameAr;
        goalStatus.NameEn = request.NameEn;
        goalStatus.DescriptionAr = request.DescriptionAr;
        goalStatus.DescriptionEn = request.DescriptionEn;
        goalStatus.DisplayOrder = request.DisplayOrder;
        goalStatus.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

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
            Message = "Goal status updated successfully",
            Data = dto
        };
    }
}
