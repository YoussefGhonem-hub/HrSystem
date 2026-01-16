using ErrorOr;
using HrSystem.Application.Features.Performance.KPIs.Common;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Performance.KPIs.Queries.GetKPIById;

public record GetKPIByIdQuery(Guid Id) : IRequest<ErrorOr<GenericResponse<KPIDto>>>;
