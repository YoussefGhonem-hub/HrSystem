using ErrorOr;
using HrSystem.Application.Features.Performance.Feedbacks.Common;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Performance.Feedbacks.Commands.UpdateFeedback;

public record UpdateFeedbackCommand(
    Guid Id,
    string FeedbackType,
    decimal Rating,
    string? Comments,
    bool IsAnonymous
) : IRequest<ErrorOr<GenericResponse<FeedbackDto>>>;
