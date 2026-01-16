using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.PerformanceReviews.Queries.GetPerformanceReviewById;

public class GetPerformanceReviewByIdQueryHandler : IRequestHandler<GetPerformanceReviewByIdQuery, ErrorOr<GenericResponse<PerformanceReviewDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetPerformanceReviewByIdQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<PerformanceReviewDto>>> Handle(
        GetPerformanceReviewByIdQuery request,
        CancellationToken cancellationToken)
    {
        var review = await _context.PerformanceReviews
            .Include(r => r.Employee)
            .Include(r => r.Reviewer)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (review == null)
        {
            return Error.NotFound(description: "Performance review not found");
        }

        var dto = new PerformanceReviewDto
        {
            Id = review.Id,
            EmployeeId = review.EmployeeId,
            EmployeeName = review.Employee?.FullNameEn,
            ReviewerId = review.ReviewerId,
            ReviewerName = review.Reviewer?.FullNameEn,
            ReviewPeriodStart = review.ReviewPeriodStart,
            ReviewPeriodEnd = review.ReviewPeriodEnd,
            ReviewDate = review.ReviewDate,
            ReviewType = review.ReviewType,
            OverallRating = review.OverallRating,
            Status = review.Status,
            StrengthsAr = review.StrengthsAr,
            StrengthsEn = review.StrengthsEn,
            WeaknessesAr = review.WeaknessesAr,
            WeaknessesEn = review.WeaknessesEn,
            ImprovementAreasAr = review.ImprovementAreasAr,
            ImprovementAreasEn = review.ImprovementAreasEn,
            CommentsAr = review.CommentsAr,
            CommentsEn = review.CommentsEn,
            EmployeeAcknowledged = review.EmployeeAcknowledged,
            EmployeeAcknowledgedDate = review.EmployeeAcknowledgedDate,
            EmployeeComments = review.EmployeeComments
        };

        return new GenericResponse<PerformanceReviewDto>
        {
            Success = true,
            Data = dto
        };
    }
}
