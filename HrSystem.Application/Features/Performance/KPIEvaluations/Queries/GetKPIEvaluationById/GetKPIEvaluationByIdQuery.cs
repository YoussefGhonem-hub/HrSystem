using ErrorOr;
using HrSystem.Application.Features.Performance.KPIEvaluations.Common;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Performance.KPIEvaluations.Queries.GetKPIEvaluationById;

public record GetKPIEvaluationByIdQuery(Guid Id) : IRequest<ErrorOr<GenericResponse<KPIEvaluationDto>>>;
