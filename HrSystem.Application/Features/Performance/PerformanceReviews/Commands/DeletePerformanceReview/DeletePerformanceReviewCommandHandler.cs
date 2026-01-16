using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.PerformanceReviews.Commands.DeletePerformanceReview;

public class DeletePerformanceReviewCommandHandler : IRequestHandler<DeletePerformanceReviewCommand, ErrorOr<GenericResponse>>
{
    private readonly ApplicationDbContext _context;

    public DeletePerformanceReviewCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse>> Handle(
        DeletePerformanceReviewCommand request,
        CancellationToken cancellationToken)
    {
        var review = await _context.PerformanceReviews
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (review == null)
        {
            return Error.NotFound(description: "Performance review not found");
        }

        _context.PerformanceReviews.Remove(review);
        await _context.SaveChangesAsync(cancellationToken);

        return new GenericResponse
        {
            Success = true,
            Message = "Performance review deleted successfully"
        };
    }
}
