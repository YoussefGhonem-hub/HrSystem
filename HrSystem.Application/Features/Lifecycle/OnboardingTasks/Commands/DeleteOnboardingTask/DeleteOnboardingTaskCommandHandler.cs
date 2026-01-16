using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Lifecycle.OnboardingTasks.Commands.DeleteOnboardingTask;

public class DeleteOnboardingTaskCommandHandler : IRequestHandler<DeleteOnboardingTaskCommand, ErrorOr<GenericResponse>>
{
    private readonly ApplicationDbContext _context;

    public DeleteOnboardingTaskCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse>> Handle(
        DeleteOnboardingTaskCommand request,
        CancellationToken cancellationToken)
    {
        var task = await _context.OnboardingTasks
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (task == null)
        {
            return Error.NotFound(description: "Onboarding task not found");
        }

        _context.OnboardingTasks.Remove(task);
        await _context.SaveChangesAsync(cancellationToken);

        return new GenericResponse
        {
            Success = true,
            Message = "Onboarding task deleted successfully"
        };
    }
}
