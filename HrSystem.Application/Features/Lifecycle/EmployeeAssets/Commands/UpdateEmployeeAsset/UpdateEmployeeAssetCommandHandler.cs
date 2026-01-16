using ErrorOr;
using HrSystem.Application.Features.Lifecycle.EmployeeAssets.Queries.GetEmployeeAssetById;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Lifecycle.EmployeeAssets.Commands.UpdateEmployeeAsset;

public class UpdateEmployeeAssetCommandHandler : IRequestHandler<UpdateEmployeeAssetCommand, ErrorOr<GenericResponse<EmployeeAssetDto>>>
{
    private readonly ApplicationDbContext _context;

    public UpdateEmployeeAssetCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<EmployeeAssetDto>>> Handle(
        UpdateEmployeeAssetCommand request,
        CancellationToken cancellationToken)
    {
        var asset = await _context.EmployeeAssets
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (asset == null)
        {
            return Error.NotFound(description: "Employee asset not found");
        }

        asset.AssetType = request.AssetType;
        asset.AssetName = request.AssetName;
        asset.SerialNumber = request.SerialNumber;
        asset.Model = request.Model;
        asset.Description = request.Description;
        asset.AssignedDate = request.AssignedDate;
        asset.ReturnDate = request.ReturnDate;
        asset.IsReturned = request.IsReturned;
        asset.Value = request.Value;
        asset.Condition = request.Condition;
        asset.ReturnNotes = request.ReturnNotes;

        await _context.SaveChangesAsync(cancellationToken);

        var updatedAsset = await _context.EmployeeAssets
            .Include(a => a.Employee)
            .FirstAsync(a => a.Id == asset.Id, cancellationToken);

        var dto = new EmployeeAssetDto
        {
            Id = updatedAsset.Id,
            EmployeeId = updatedAsset.EmployeeId,
            EmployeeName = updatedAsset.Employee?.FullNameEn,
            AssetType = updatedAsset.AssetType,
            AssetName = updatedAsset.AssetName,
            SerialNumber = updatedAsset.SerialNumber,
            Model = updatedAsset.Model,
            Description = updatedAsset.Description,
            AssignedDate = updatedAsset.AssignedDate,
            ReturnDate = updatedAsset.ReturnDate,
            IsReturned = updatedAsset.IsReturned,
            Value = updatedAsset.Value,
            Condition = updatedAsset.Condition,
            ReturnNotes = updatedAsset.ReturnNotes
        };

        return new GenericResponse<EmployeeAssetDto>
        {
            Success = true,
            Message = "Employee asset updated successfully",
            Data = dto
        };
    }
}
