using ErrorOr;
using HrSystem.Application.Features.Performance.Feedbacks.Common;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Performance.Feedbacks.Queries.GetFeedbackById;

public record GetFeedbackByIdQuery(Guid Id) : IRequest<ErrorOr<GenericResponse<FeedbackDto>>>;
