using ErrorOr;
using HrSystem.Application.Features.Performance.PerformanceReviews.Queries.GetPerformanceReviewById;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.PerformanceReviews.Commands.UpdatePerformanceReview;

public class UpdatePerformanceReviewCommandHandler : IRequestHandler<UpdatePerformanceReviewCommand, ErrorOr<GenericResponse<PerformanceReviewDto>>>
{
    private readonly ApplicationDbContext _context;

    public UpdatePerformanceReviewCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<PerformanceReviewDto>>> Handle(
        UpdatePerformanceReviewCommand request,
        CancellationToken cancellationToken)
    {
        var review = await _context.PerformanceReviews
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (review == null)
        {
            return Error.NotFound(description: "Performance review not found");
        }

        review.ReviewPeriodStart = request.ReviewPeriodStart;
        review.ReviewPeriodEnd = request.ReviewPeriodEnd;
        review.ReviewDate = request.ReviewDate;
        review.ReviewTypeId = request.ReviewTypeId;
        review.OverallRating = request.OverallRating;
        review.StatusId = request.StatusId;
        review.StrengthsAr = request.StrengthsAr;
        review.StrengthsEn = request.StrengthsEn;
        review.WeaknessesAr = request.WeaknessesAr;
        review.WeaknessesEn = request.WeaknessesEn;
        review.ImprovementAreasAr = request.ImprovementAreasAr;
        review.ImprovementAreasEn = request.ImprovementAreasEn;
        review.CommentsAr = request.CommentsAr;
        review.CommentsEn = request.CommentsEn;
        review.EmployeeComments = request.EmployeeComments;

        if (request.EmployeeAcknowledged && !review.EmployeeAcknowledged)
        {
            review.EmployeeAcknowledged = true;
            review.EmployeeAcknowledgedDate = DateTime.UtcNow;
        }
        else if (!request.EmployeeAcknowledged && review.EmployeeAcknowledged)
        {
            review.EmployeeAcknowledged = false;
            review.EmployeeAcknowledgedDate = null;
        }

        await _context.SaveChangesAsync(cancellationToken);

        var updatedReview = await _context.PerformanceReviews
            .Include(r => r.Employee)
            .Include(r => r.Reviewer)
            .Include(r => r.ReviewType)
            .Include(r => r.Status)
            .FirstAsync(r => r.Id == review.Id, cancellationToken);

        var dto = new PerformanceReviewDto
        {
            Id = updatedReview.Id,
            EmployeeId = updatedReview.EmployeeId,
            EmployeeName = updatedReview.Employee?.FullNameEn,
            ReviewerId = updatedReview.ReviewerId,
            ReviewerName = updatedReview.Reviewer?.FullNameEn,
            ReviewPeriodStart = updatedReview.ReviewPeriodStart,
            ReviewPeriodEnd = updatedReview.ReviewPeriodEnd,
            ReviewDate = updatedReview.ReviewDate,
            ReviewTypeId = updatedReview.ReviewTypeId,
            ReviewType = updatedReview.ReviewType?.NameEn,
            OverallRating = updatedReview.OverallRating,
            StatusId = updatedReview.StatusId,
            Status = updatedReview.Status?.NameEn,
            StrengthsAr = updatedReview.StrengthsAr,
            StrengthsEn = updatedReview.StrengthsEn,
            WeaknessesAr = updatedReview.WeaknessesAr,
            WeaknessesEn = updatedReview.WeaknessesEn,
            ImprovementAreasAr = updatedReview.ImprovementAreasAr,
            ImprovementAreasEn = updatedReview.ImprovementAreasEn,
            CommentsAr = updatedReview.CommentsAr,
            CommentsEn = updatedReview.CommentsEn,
            EmployeeAcknowledged = updatedReview.EmployeeAcknowledged,
            EmployeeAcknowledgedDate = updatedReview.EmployeeAcknowledgedDate,
            EmployeeComments = updatedReview.EmployeeComments
        };

        return new GenericResponse<PerformanceReviewDto>
        {
            Success = true,
            Message = "Performance review updated successfully",
            Data = dto
        };
    }
}
