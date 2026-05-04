using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Storage.AWS3.Services;

namespace HrSystem.Application.Features.Lifecycle.EmployeeAssets.Queries.GetEmployeeAssetById;

public class GetEmployeeAssetByIdQueryHandler : IRequestHandler<GetEmployeeAssetByIdQuery, ErrorOr<GenericResponse<EmployeeAssetDto>>>
{
    private readonly ApplicationDbContext _context;
    private readonly IStorageService _storageService;

    public GetEmployeeAssetByIdQueryHandler(ApplicationDbContext context, IStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    public async Task<ErrorOr<GenericResponse<EmployeeAssetDto>>> Handle(
        GetEmployeeAssetByIdQuery request,
        CancellationToken cancellationToken)
    {
        var asset = await _context.EmployeeAssets
            .Include(a => a.Employee)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (asset == null)
        {
            return Error.NotFound(description: "Employee asset not found");
        }

        var resolvedImageUrl = !string.IsNullOrWhiteSpace(asset.ImageUrl)
            ? await _storageService.DownloadFileUrl(asset.ImageUrl, cancellationToken)
            : null;

        var dto = new EmployeeAssetDto
        {
            Id = asset.Id,
            EmployeeId = asset.EmployeeId,
            EmployeeName = asset.Employee?.FullNameEn,
            AssetType = asset.AssetType,
            AssetName = asset.AssetName,
            SerialNumber = asset.SerialNumber,
            Model = asset.Model,
            Description = asset.Description,
            AssignedDate = asset.AssignedDate,
            ReturnDate = asset.ReturnDate,
            IsReturned = asset.IsReturned,
            Value = asset.Value,
            Condition = asset.Condition,
            ReturnNotes = asset.ReturnNotes,
            ImageUrl = resolvedImageUrl
        };

        return new GenericResponse<EmployeeAssetDto>
        {
            Success = true,
            Data = dto
        };
    }
}
