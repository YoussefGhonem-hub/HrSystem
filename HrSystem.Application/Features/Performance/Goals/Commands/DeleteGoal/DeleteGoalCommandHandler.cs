using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.Goals.Commands.DeleteGoal;

public class DeleteGoalCommandHandler : IRequestHandler<DeleteGoalCommand, ErrorOr<GenericResponse>>
{
    private readonly ApplicationDbContext _context;

    public DeleteGoalCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse>> Handle(
        DeleteGoalCommand request,
        CancellationToken cancellationToken)
    {
        var goal = await _context.Goals
            .FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken);

        if (goal == null)
        {
            return Error.NotFound(description: "Goal not found");
        }

        _context.Goals.Remove(goal);
        await _context.SaveChangesAsync(cancellationToken);

        return new GenericResponse
        {
            Success = true,
            Message = "Goal deleted successfully"
        };
    }
}
