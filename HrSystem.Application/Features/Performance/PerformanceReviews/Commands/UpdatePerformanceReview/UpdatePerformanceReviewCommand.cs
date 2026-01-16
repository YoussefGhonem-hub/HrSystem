using ErrorOr;
using HrSystem.Application.Features.Performance.PerformanceReviews.Queries.GetPerformanceReviewById;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Performance.PerformanceReviews.Commands.UpdatePerformanceReview;

public record UpdatePerformanceReviewCommand(
    Guid Id,
    DateTime ReviewPeriodStart,
    DateTime ReviewPeriodEnd,
    DateTime ReviewDate,
    string ReviewType,
    decimal OverallRating,
    string Status,
    string? StrengthsAr,
    string? StrengthsEn,
    string? WeaknessesAr,
    string? WeaknessesEn,
    string? ImprovementAreasAr,
    string? ImprovementAreasEn,
    string? CommentsAr,
    string? CommentsEn,
    bool EmployeeAcknowledged,
    string? EmployeeComments
) : IRequest<ErrorOr<GenericResponse<PerformanceReviewDto>>>;
