using ErrorOr;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Performance.KPIEvaluations.Commands.DeleteKPIEvaluation;

public record DeleteKPIEvaluationCommand(Guid Id) : IRequest<ErrorOr<GenericResponse>>;
