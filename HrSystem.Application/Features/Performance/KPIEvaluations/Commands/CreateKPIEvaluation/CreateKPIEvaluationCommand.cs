using ErrorOr;
using HrSystem.Application.Features.Performance.KPIEvaluations.Common;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Performance.KPIEvaluations.Commands.CreateKPIEvaluation;

public record CreateKPIEvaluationCommand(
    Guid PerformanceReviewId,
    Guid KPIId,
    decimal Rating,
    decimal WeightedScore,
    string? Comments,
    string? Evidence
) : IRequest<ErrorOr<GenericResponse<KPIEvaluationDto>>>;
