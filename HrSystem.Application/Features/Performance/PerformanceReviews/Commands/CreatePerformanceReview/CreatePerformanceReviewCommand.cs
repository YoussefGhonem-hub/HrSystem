using ErrorOr;
using HrSystem.Application.Features.Performance.PerformanceReviews.Queries.GetPerformanceReviewById;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.PerformanceReviews.Commands.CreatePerformanceReview;

public record CreatePerformanceReviewCommand(
    Guid EmployeeId,
    Guid ReviewerId,
    DateTime ReviewPeriodStart,
    DateTime ReviewPeriodEnd,
    DateTime ReviewDate,
    string ReviewType,
    decimal OverallRating,
    string? StrengthsAr,
    string? StrengthsEn,
    string? WeaknessesAr,
    string? WeaknessesEn,
    string? ImprovementAreasAr,
    string? ImprovementAreasEn,
    string? CommentsAr,
    string? CommentsEn
) : IRequest<ErrorOr<GenericResponse<PerformanceReviewDto>>>;

public class CreatePerformanceReviewCommandHandler : IRequestHandler<CreatePerformanceReviewCommand, ErrorOr<GenericResponse<PerformanceReviewDto>>>
{
    private readonly ApplicationDbContext _context;

    public CreatePerformanceReviewCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<PerformanceReviewDto>>> Handle(
        CreatePerformanceReviewCommand request,
        CancellationToken cancellationToken)
    {
        var review = new Domain.Entities.Performance.PerformanceReview
        {
            EmployeeId = request.EmployeeId,
            ReviewerId = request.ReviewerId,
            ReviewPeriodStart = request.ReviewPeriodStart,
            ReviewPeriodEnd = request.ReviewPeriodEnd,
            ReviewDate = request.ReviewDate,
            ReviewType = request.ReviewType,
            OverallRating = request.OverallRating,
            Status = "Draft",
            StrengthsAr = request.StrengthsAr,
            StrengthsEn = request.StrengthsEn,
            WeaknessesAr = request.WeaknessesAr,
            WeaknessesEn = request.WeaknessesEn,
            ImprovementAreasAr = request.ImprovementAreasAr,
            ImprovementAreasEn = request.ImprovementAreasEn,
            CommentsAr = request.CommentsAr,
            CommentsEn = request.CommentsEn,
            EmployeeAcknowledged = false,
            TenantId = Guid.NewGuid()
        };

        _context.PerformanceReviews.Add(review);
        await _context.SaveChangesAsync(cancellationToken);

        var createdReview = await _context.PerformanceReviews
            .Include(r => r.Employee)
            .Include(r => r.Reviewer)
            .FirstAsync(r => r.Id == review.Id, cancellationToken);

        var dto = new PerformanceReviewDto
        {
            Id = createdReview.Id,
            EmployeeId = createdReview.EmployeeId,
            EmployeeName = createdReview.Employee?.FullNameEn,
            ReviewerId = createdReview.ReviewerId,
            ReviewerName = createdReview.Reviewer?.FullNameEn,
            ReviewPeriodStart = createdReview.ReviewPeriodStart,
            ReviewPeriodEnd = createdReview.ReviewPeriodEnd,
            ReviewDate = createdReview.ReviewDate,
            ReviewType = createdReview.ReviewType,
            OverallRating = createdReview.OverallRating,
            Status = createdReview.Status,
            StrengthsAr = createdReview.StrengthsAr,
            StrengthsEn = createdReview.StrengthsEn,
            WeaknessesAr = createdReview.WeaknessesAr,
            WeaknessesEn = createdReview.WeaknessesEn,
            ImprovementAreasAr = createdReview.ImprovementAreasAr,
            ImprovementAreasEn = createdReview.ImprovementAreasEn,
            CommentsAr = createdReview.CommentsAr,
            CommentsEn = createdReview.CommentsEn,
            EmployeeAcknowledged = createdReview.EmployeeAcknowledged,
            EmployeeAcknowledgedDate = createdReview.EmployeeAcknowledgedDate,
            EmployeeComments = createdReview.EmployeeComments
        };

        return new GenericResponse<PerformanceReviewDto>
        {
            Success = true,
            Message = "Performance review created successfully",
            Data = dto
        };
    }
}
