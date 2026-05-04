using ErrorOr;
using HrSystem.Application.Features.Lifecycle.EmployeeAssets.Queries.GetEmployeeAssetById;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Storage.AWS3.Services;

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
    string Condition,
    IFormFile? Image
) : IRequest<ErrorOr<GenericResponse<EmployeeAssetDto>>>;

public class CreateEmployeeAssetCommandHandler : IRequestHandler<CreateEmployeeAssetCommand, ErrorOr<GenericResponse<EmployeeAssetDto>>>
{
    private readonly ApplicationDbContext _context;
    private readonly IStorageService _storageService;

    public CreateEmployeeAssetCommandHandler(ApplicationDbContext context, IStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    public async Task<ErrorOr<GenericResponse<EmployeeAssetDto>>> Handle(
        CreateEmployeeAssetCommand request,
        CancellationToken cancellationToken)
    {
        string? imageKey = null;
        if (request.Image != null && request.Image.Length > 0)
        {
            var stored = await _storageService.Upload(request.Image, cancellationToken);
            if (stored == null || string.IsNullOrWhiteSpace(stored.Key))
            {
                return Error.Failure("EmployeeAsset.ImageUploadFailed", "Failed to upload asset image");
            }
            imageKey = stored.Key;
        }

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
            ImageUrl = imageKey,
            TenantId = Guid.Empty
        };

        _context.EmployeeAssets.Add(asset);
        await _context.SaveChangesAsync(cancellationToken);

        var createdAsset = await _context.EmployeeAssets
            .Include(a => a.Employee)
            .FirstAsync(a => a.Id == asset.Id, cancellationToken);

        var resolvedImageUrl = !string.IsNullOrWhiteSpace(createdAsset.ImageUrl)
            ? await _storageService.DownloadFileUrl(createdAsset.ImageUrl, cancellationToken)
            : null;

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
            Condition = createdAsset.Condition,
            ImageUrl = resolvedImageUrl
        };

        return new GenericResponse<EmployeeAssetDto>
        {
            Success = true,
            Message = "Employee asset created successfully",
            Data = dto
        };
    }
}
