using ErrorOr;
using HrSystem.Application.Features.Performance.Feedbacks.Common;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Performance.Feedbacks.Commands.CreateFeedback;

public record CreateFeedbackCommand(
    Guid PerformanceReviewId,
    Guid ProvidedBy,
    string FeedbackType,
    decimal Rating,
    string? Comments,
    bool IsAnonymous
) : IRequest<ErrorOr<GenericResponse<FeedbackDto>>>;
