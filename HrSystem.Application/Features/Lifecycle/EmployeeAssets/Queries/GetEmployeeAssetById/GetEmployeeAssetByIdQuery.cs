using ErrorOr;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Lifecycle.EmployeeAssets.Queries.GetEmployeeAssetById;

public record GetEmployeeAssetByIdQuery(Guid Id) : IRequest<ErrorOr<GenericResponse<EmployeeAssetDto>>>;
