using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.GoalPriorities.Commands.DeleteGoalPriority;

public class DeleteGoalPriorityCommandHandler : IRequestHandler<DeleteGoalPriorityCommand, ErrorOr<GenericResponse>>
{
    private readonly ApplicationDbContext _context;

    public DeleteGoalPriorityCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse>> Handle(
        DeleteGoalPriorityCommand request,
        CancellationToken cancellationToken)
    {
        var goalPriority = await _context.GoalPriorities
            .Include(gp => gp.Goals)
            .FirstOrDefaultAsync(gp => gp.Id == request.Id, cancellationToken);

        if (goalPriority == null)
        {
            return Error.NotFound(description: "Goal priority not found");
        }

        // Check if goal priority is used by any goals
        if (goalPriority.Goals.Any())
        {
            return Error.Conflict(description: "Cannot delete goal priority that is assigned to goals");
        }

        // Soft delete by setting IsDeleted to true
        goalPriority.IsDeleted = true;
        goalPriority.DeletedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new GenericResponse
        {
            Success = true,
            Message = "Goal priority deleted successfully"
        };
    }
}
