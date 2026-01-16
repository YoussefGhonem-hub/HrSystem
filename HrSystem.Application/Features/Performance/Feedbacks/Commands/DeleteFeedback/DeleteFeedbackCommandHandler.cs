using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.Feedbacks.Commands.DeleteFeedback;

public class DeleteFeedbackCommandHandler : IRequestHandler<DeleteFeedbackCommand, ErrorOr<GenericResponse>>
{
    private readonly ApplicationDbContext _context;

    public DeleteFeedbackCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse>> Handle(
        DeleteFeedbackCommand request,
        CancellationToken cancellationToken)
    {
        var feedback = await _context.Feedbacks
            .FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);

        if (feedback == null)
        {
            return Error.NotFound(description: "Feedback not found");
        }

        _context.Feedbacks.Remove(feedback);
        await _context.SaveChangesAsync(cancellationToken);

        return new GenericResponse
        {
            Success = true,
            Message = "Feedback deleted successfully"
        };
    }
}
