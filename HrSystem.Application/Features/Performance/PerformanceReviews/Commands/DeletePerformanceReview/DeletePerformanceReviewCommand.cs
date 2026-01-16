using ErrorOr;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Performance.PerformanceReviews.Commands.DeletePerformanceReview;

public record DeletePerformanceReviewCommand(Guid Id) : IRequest<ErrorOr<GenericResponse>>;
