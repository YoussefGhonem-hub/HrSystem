using ErrorOr;
using HrSystem.Application.Features.Performance.Feedbacks.Common;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.Feedbacks.Queries.GetFeedbackById;

public class GetFeedbackByIdQueryHandler : IRequestHandler<GetFeedbackByIdQuery, ErrorOr<GenericResponse<FeedbackDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetFeedbackByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<FeedbackDto>>> Handle(
        GetFeedbackByIdQuery request,
        CancellationToken cancellationToken)
    {
        var feedback = await _context.Feedbacks
            .Include(f => f.PerformanceReview)
                .ThenInclude(pr => pr.Employee)
            .Include(f => f.Provider)
            .FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);

        if (feedback == null)
        {
            return Error.NotFound(description: "Feedback not found");
        }

        var dto = new FeedbackDto
        {
            Id = feedback.Id,
            PerformanceReviewId = feedback.PerformanceReviewId,
            EmployeeName = feedback.PerformanceReview.Employee.FullNameEn,
            ProvidedBy = feedback.ProvidedBy,
            ProviderName = feedback.IsAnonymous ? "Anonymous" : feedback.Provider.FullNameEn,
            FeedbackType = feedback.FeedbackType,
            Rating = feedback.Rating,
            Comments = feedback.Comments,
            IsAnonymous = feedback.IsAnonymous,
            CreatedDate = feedback.CreatedDate.DateTime
        };

        return new GenericResponse<FeedbackDto>
        {
            Success = true,
            Data = dto
        };
    }
}
