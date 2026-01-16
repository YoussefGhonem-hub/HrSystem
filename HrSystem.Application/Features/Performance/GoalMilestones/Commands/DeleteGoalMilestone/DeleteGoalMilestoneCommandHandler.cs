using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.GoalMilestones.Commands.DeleteGoalMilestone;

public class DeleteGoalMilestoneCommandHandler : IRequestHandler<DeleteGoalMilestoneCommand, ErrorOr<GenericResponse>>
{
    private readonly ApplicationDbContext _context;

    public DeleteGoalMilestoneCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse>> Handle(
        DeleteGoalMilestoneCommand request,
        CancellationToken cancellationToken)
    {
        var goalMilestone = await _context.GoalMilestones
            .FirstOrDefaultAsync(gm => gm.Id == request.Id, cancellationToken);

        if (goalMilestone == null)
        {
            return Error.NotFound(description: "Goal milestone not found");
        }

        _context.GoalMilestones.Remove(goalMilestone);
        await _context.SaveChangesAsync(cancellationToken);

        return new GenericResponse
        {
            Success = true,
            Message = "Goal milestone deleted successfully"
        };
    }
}
