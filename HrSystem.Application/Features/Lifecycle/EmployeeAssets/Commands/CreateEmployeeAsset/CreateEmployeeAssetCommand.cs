using ErrorOr;
using HrSystem.Application.Features.Lifecycle.EmployeeAssets.Queries.GetEmployeeAssetById;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Lifecycle.EmployeeAssets.Commands.CreateEmployeeAsset;

public record CreateEmployeeAssetCommand(
    Guid EmployeeId,
    string AssetType,
    string AssetName,
    string? SerialNumber,
    string? Model,
    string? Description,
    DateTime AssignedDate,
    decimal? Value,
    string Condition
) : IRequest<ErrorOr<GenericResponse<EmployeeAssetDto>>>;

public class CreateEmployeeAssetCommandHandler : IRequestHandler<CreateEmployeeAssetCommand, ErrorOr<GenericResponse<EmployeeAssetDto>>>
{
    private readonly ApplicationDbContext _context;

    public CreateEmployeeAssetCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<EmployeeAssetDto>>> Handle(
        CreateEmployeeAssetCommand request,
        CancellationToken cancellationToken)
    {
        var asset = new Domain.Entities.Lifecycle.EmployeeAsset
        {
            EmployeeId = request.EmployeeId,
            AssetType = request.AssetType,
            AssetName = request.AssetName,
            SerialNumber = request.SerialNumber,
            Model = request.Model,
            Description = request.Description,
            AssignedDate = request.AssignedDate,
            Value = request.Value,
            Condition = request.Condition,
            IsReturned = false,
            TenantId = Guid.Empty
        };

        _context.EmployeeAssets.Add(asset);
        await _context.SaveChangesAsync(cancellationToken);

        var createdAsset = await _context.EmployeeAssets
            .Include(a => a.Employee)
            .FirstAsync(a => a.Id == asset.Id, cancellationToken);

        var dto = new EmployeeAssetDto
        {
            Id = createdAsset.Id,
            EmployeeId = createdAsset.EmployeeId,
            EmployeeName = createdAsset.Employee?.FullNameEn,
            AssetType = createdAsset.AssetType,
            AssetName = createdAsset.AssetName,
            SerialNumber = createdAsset.SerialNumber,
            Model = createdAsset.Model,
            Description = createdAsset.Description,
            AssignedDate = createdAsset.AssignedDate,
            ReturnDate = createdAsset.ReturnDate,
            IsReturned = createdAsset.IsReturned,
            Value = createdAsset.Value,
            Condition = createdAsset.Condition
        };

        return new GenericResponse<EmployeeAssetDto>
        {
            Success = true,
            Message = "Employee asset created successfully",
            Data = dto
        };
    }
}
