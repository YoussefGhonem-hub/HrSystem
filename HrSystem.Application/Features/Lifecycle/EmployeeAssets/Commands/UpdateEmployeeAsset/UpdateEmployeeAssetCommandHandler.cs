using ErrorOr;
using HrSystem.Application.Features.Lifecycle.EmployeeAssets.Queries.GetEmployeeAssetById;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Storage.AWS3.Services;

namespace HrSystem.Application.Features.Lifecycle.EmployeeAssets.Commands.UpdateEmployeeAsset;

public class UpdateEmployeeAssetCommandHandler : IRequestHandler<UpdateEmployeeAssetCommand, ErrorOr<GenericResponse<EmployeeAssetDto>>>
{
    private readonly ApplicationDbContext _context;
    private readonly IStorageService _storageService;

    public UpdateEmployeeAssetCommandHandler(ApplicationDbContext context, IStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

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

        if (request.Image != null && request.Image.Length > 0)
        {
            var stored = await _storageService.Upload(request.Image, cancellationToken);
            if (stored == null || string.IsNullOrWhiteSpace(stored.Key))
            {
                return Error.Failure("EmployeeAsset.ImageUploadFailed", "Failed to upload asset image");
            }
            asset.ImageUrl = stored.Key;
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

        var resolvedImageUrl = !string.IsNullOrWhiteSpace(updatedAsset.ImageUrl)
            ? await _storageService.DownloadFileUrl(updatedAsset.ImageUrl, cancellationToken)
            : null;

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
            ReturnNotes = updatedAsset.ReturnNotes,
            ImageUrl = resolvedImageUrl
        };

        return new GenericResponse<EmployeeAssetDto>
        {
            Success = true,
            Message = "Employee asset updated successfully",
            Data = dto
        };
    }
}
