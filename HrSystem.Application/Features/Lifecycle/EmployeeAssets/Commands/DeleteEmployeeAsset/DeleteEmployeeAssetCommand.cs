using ErrorOr;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Lifecycle.EmployeeAssets.Commands.DeleteEmployeeAsset;

public record DeleteEmployeeAssetCommand(Guid Id) : IRequest<ErrorOr<GenericResponse>>;
