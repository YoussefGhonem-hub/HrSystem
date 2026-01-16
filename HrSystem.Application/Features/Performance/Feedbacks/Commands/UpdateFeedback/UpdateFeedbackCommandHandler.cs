using ErrorOr;
using HrSystem.Application.Features.Performance.Feedbacks.Common;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.Feedbacks.Commands.UpdateFeedback;

public class UpdateFeedbackCommandHandler : IRequestHandler<UpdateFeedbackCommand, ErrorOr<GenericResponse<FeedbackDto>>>
{
    private readonly ApplicationDbContext _context;

    public UpdateFeedbackCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<FeedbackDto>>> Handle(
        UpdateFeedbackCommand request,
        CancellationToken cancellationToken)
    {
        var feedback = await _context.Feedbacks
            .FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);

        if (feedback == null)
        {
            return Error.NotFound(description: "Feedback not found");
        }

        feedback.FeedbackType = request.FeedbackType;
        feedback.Rating = request.Rating;
        feedback.Comments = request.Comments;
        feedback.IsAnonymous = request.IsAnonymous;

        await _context.SaveChangesAsync(cancellationToken);

        // Reload with navigation properties
        var updatedFeedback = await _context.Feedbacks
            .Include(f => f.PerformanceReview)
                .ThenInclude(pr => pr.Employee)
            .Include(f => f.Provider)
            .FirstAsync(f => f.Id == feedback.Id, cancellationToken);

        var dto = new FeedbackDto
        {
            Id = updatedFeedback.Id,
            PerformanceReviewId = updatedFeedback.PerformanceReviewId,
            EmployeeName = updatedFeedback.PerformanceReview.Employee.FullNameEn,
            ProvidedBy = updatedFeedback.ProvidedBy,
            ProviderName = updatedFeedback.IsAnonymous ? "Anonymous" : updatedFeedback.Provider.FullNameEn,
            FeedbackType = updatedFeedback.FeedbackType,
            Rating = updatedFeedback.Rating,
            Comments = updatedFeedback.Comments,
            IsAnonymous = updatedFeedback.IsAnonymous,
            CreatedDate = updatedFeedback.CreatedDate.DateTime
        };

        return new GenericResponse<FeedbackDto>
        {
            Success = true,
            Data = dto,
            Message = "Feedback updated successfully"
        };
    }
}
