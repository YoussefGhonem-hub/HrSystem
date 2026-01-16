using ErrorOr;
using HrSystem.Application.Features.Performance.KPIEvaluations.Common;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Performance.KPIEvaluations.Commands.UpdateKPIEvaluation;

public record UpdateKPIEvaluationCommand(
    Guid Id,
    decimal Rating,
    decimal WeightedScore,
    string? Comments,
    string? Evidence
) : IRequest<ErrorOr<GenericResponse<KPIEvaluationDto>>>;
