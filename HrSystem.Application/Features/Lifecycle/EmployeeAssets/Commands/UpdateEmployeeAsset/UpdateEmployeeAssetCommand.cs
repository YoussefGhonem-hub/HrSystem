using ErrorOr;
using HrSystem.Application.Features.Lifecycle.EmployeeAssets.Queries.GetEmployeeAssetById;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Lifecycle.EmployeeAssets.Commands.UpdateEmployeeAsset;

public record UpdateEmployeeAssetCommand(
    Guid Id,
    string AssetType,
    string AssetName,
    string? SerialNumber,
    string? Model,
    string? Description,
    DateTime AssignedDate,
    DateTime? ReturnDate,
    bool IsReturned,
    decimal? Value,
    string Condition,
    string? ReturnNotes
) : IRequest<ErrorOr<GenericResponse<EmployeeAssetDto>>>;
