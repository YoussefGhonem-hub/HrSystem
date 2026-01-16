using ErrorOr;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Performance.PerformanceReviews.Queries.GetPerformanceReviewById;

public record GetPerformanceReviewByIdQuery(Guid Id) : IRequest<ErrorOr<GenericResponse<PerformanceReviewDto>>>;
