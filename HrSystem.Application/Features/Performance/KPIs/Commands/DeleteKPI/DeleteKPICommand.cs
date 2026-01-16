using ErrorOr;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Performance.KPIs.Commands.DeleteKPI;

public record DeleteKPICommand(Guid Id) : IRequest<ErrorOr<GenericResponse>>;
