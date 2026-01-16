using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Lifecycle.OffboardingTasks.Commands.DeleteOffboardingTask;

public class DeleteOffboardingTaskCommandHandler : IRequestHandler<DeleteOffboardingTaskCommand, ErrorOr<GenericResponse>>
{
    private readonly ApplicationDbContext _context;

    public DeleteOffboardingTaskCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse>> Handle(
        DeleteOffboardingTaskCommand request,
        CancellationToken cancellationToken)
    {
        var task = await _context.OffboardingTasks
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (task == null)
        {
            return Error.NotFound(description: "Offboarding task not found");
        }

        _context.OffboardingTasks.Remove(task);
        await _context.SaveChangesAsync(cancellationToken);

        return new GenericResponse
        {
            Success = true,
            Message = "Offboarding task deleted successfully"
        };
    }
}
