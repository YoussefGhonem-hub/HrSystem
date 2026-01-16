using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.GoalStatuses.Commands.DeleteGoalStatus;

public class DeleteGoalStatusCommandHandler : IRequestHandler<DeleteGoalStatusCommand, ErrorOr<GenericResponse>>
{
    private readonly ApplicationDbContext _context;

    public DeleteGoalStatusCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse>> Handle(
        DeleteGoalStatusCommand request,
        CancellationToken cancellationToken)
    {
        var goalStatus = await _context.GoalStatuses
            .Include(gs => gs.Goals)
            .FirstOrDefaultAsync(gs => gs.Id == request.Id, cancellationToken);

        if (goalStatus == null)
        {
            return Error.NotFound(description: "Goal status not found");
        }

        // Check if goal status is used by any goals
        if (goalStatus.Goals.Any())
        {
            return Error.Conflict(description: "Cannot delete goal status that is assigned to goals");
        }

        // Soft delete by setting IsDeleted to true
        goalStatus.IsDeleted = true;
        goalStatus.DeletedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new GenericResponse
        {
            Success = true,
            Message = "Goal status deleted successfully"
        };
    }
}
