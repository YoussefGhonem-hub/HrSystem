using ErrorOr;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Performance.Feedbacks.Commands.DeleteFeedback;

public record DeleteFeedbackCommand(Guid Id) : IRequest<ErrorOr<GenericResponse>>;
