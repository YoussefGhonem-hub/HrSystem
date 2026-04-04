using ErrorOr;
using HrSystem.Application.Features.Performance.Feedbacks.Common;
using HrSystem.Domain.Entities.Performance;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.Feedbacks.Commands.CreateFeedback;

public class CreateFeedbackCommandHandler : IRequestHandler<CreateFeedbackCommand, ErrorOr<GenericResponse<FeedbackDto>>>
{
    private readonly ApplicationDbContext _context;

    public CreateFeedbackCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<FeedbackDto>>> Handle(
        CreateFeedbackCommand request,
        CancellationToken cancellationToken)
    {
        var feedback = new Feedback
        {
            PerformanceReviewId = request.PerformanceReviewId,
            ProvidedBy = request.ProvidedBy,
            FeedbackType = request.FeedbackType,
            Rating = request.Rating,
            Comments = request.Comments,
            IsAnonymous = request.IsAnonymous,
            TenantId = Guid.Empty
        };

        _context.Feedbacks.Add(feedback);
        await _context.SaveChangesAsync(cancellationToken);

        // Reload with navigation properties
        var createdFeedback = await _context.Feedbacks
            .Include(f => f.PerformanceReview)
                .ThenInclude(pr => pr.Employee)
            .Include(f => f.Provider)
            .FirstAsync(f => f.Id == feedback.Id, cancellationToken);

        var dto = new FeedbackDto
        {
            Id = createdFeedback.Id,
            PerformanceReviewId = createdFeedback.PerformanceReviewId,
            EmployeeName = createdFeedback.PerformanceReview.Employee.FullNameEn,
            ProvidedBy = createdFeedback.ProvidedBy,
            ProviderName = createdFeedback.IsAnonymous ? "Anonymous" : createdFeedback.Provider.FullNameEn,
            FeedbackType = createdFeedback.FeedbackType,
            Rating = createdFeedback.Rating,
            Comments = createdFeedback.Comments,
            IsAnonymous = createdFeedback.IsAnonymous,
            CreatedDate = createdFeedback.CreatedDate.DateTime
        };

        return new GenericResponse<FeedbackDto>
        {
            Success = true,
            Data = dto,
            Message = "Feedback created successfully"
        };
    }
}
