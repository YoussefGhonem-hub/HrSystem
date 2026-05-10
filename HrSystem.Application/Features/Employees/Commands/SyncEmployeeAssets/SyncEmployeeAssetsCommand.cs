using ErrorOr;
using HrSystem.Application.Features.Employees.Queries.GetEmployeeDetails;
using HrSystem.Domain.Entities.Lifecycle;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Employees.Commands.SyncEmployeeAssets;

/// <summary>
/// Represents a single asset in the sync request
/// - If AssetId is provided: update existing asset
/// - If AssetId is null/empty: create new asset
/// </summary>
public class SyncAssetItem
{
    /// <summary>
    /// Asset ID - if provided, update existing; if null/empty, create new
    /// </summary>
    public Guid? AssetId { get; set; }

    /// <summary>
    /// Asset type (e.g., Laptop, Phone, ID Card, Access Card)
    /// </summary>
    public string AssetType { get; set; } = string.Empty;

    /// <summary>
    /// Asset name
    /// </summary>
    public string AssetName { get; set; } = string.Empty;

    /// <summary>
    /// Serial number of the asset
    /// </summary>
    public string? SerialNumber { get; set; }

    /// <summary>
    /// Model of the asset
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Description of the asset
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Date the asset was assigned to the employee
    /// </summary>
    public DateTime AssignedDate { get; set; }

    /// <summary>
    /// Expected return date
    /// </summary>
    public DateTime? ExpectedReturnDate { get; set; }

    /// <summary>
    /// Actual return date
    /// </summary>
    public DateTime? ReturnDate { get; set; }

    /// <summary>
    /// Whether the asset has been returned
    /// </summary>
    public bool IsReturned { get; set; }

    /// <summary>
    /// Condition of the asset (Good, Fair, Poor, Damaged)
    /// </summary>
    public string Condition { get; set; } = "Good";

    /// <summary>
    /// Notes about the return
    /// </summary>
    public string? ReturnNotes { get; set; }

    /// <summary>
    /// Value of the asset
    /// </summary>
    public decimal? Value { get; set; }
}

/// <summary>
/// Command to sync employee assets:
/// - Update existing assets (with AssetId)
/// - Create new assets (without AssetId)
/// - Delete assets not included in the request (by Id)
/// </summary>
public class SyncEmployeeAssetsCommand : IRequest<ErrorOr<GenericResponse<List<EmployeeAssetDetailsDto>>>>
{
    /// <summary>
    /// The employee ID whose assets are being synced
    /// </summary>
    public Guid EmployeeId { get; set; }

    /// <summary>
    /// List of assets to sync
    /// </summary>
    public List<SyncAssetItem> Assets { get; set; } = new();

    /// <summary>
    /// List of asset IDs to remove (delete)
    /// </summary>
    public List<Guid>? AssetsToRemove { get; set; }
}

public class SyncEmployeeAssetsCommandHandler : IRequestHandler<SyncEmployeeAssetsCommand, ErrorOr<GenericResponse<List<EmployeeAssetDetailsDto>>>>
{
    private readonly ApplicationDbContext _context;

    public SyncEmployeeAssetsCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<List<EmployeeAssetDetailsDto>>>> Handle(
        SyncEmployeeAssetsCommand request,
        CancellationToken cancellationToken)
    {
        // Find employee
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId && !e.IsDeleted, cancellationToken);

        if (employee == null)
        {
            return Error.NotFound("Employee.NotFound", "Employee not found");
        }

        var editAccess = await EmployeeEditAuthorizationGuard.EnsureCanEditAsync(
            _context,
            employee.Id,
            employee.UserId,
            cancellationToken);
        if (editAccess.IsError)
        {
            return editAccess.Errors;
        }

        // Authorization check
        var isHr = CurrentUser.Roles?.Contains(RoleNames.HRManager) == true ||
                   CurrentUser.Roles?.Contains(RoleNames.HRSpecialist) == true ||
                   CurrentUser.Roles?.Contains(RoleNames.OrganizationAdmin) == true ||
                   CurrentUser.Roles?.Contains(RoleNames.SuperAdmin) == true;

        if (!isHr)
        {
            return Error.Unauthorized("Assets.Unauthorized", "Not allowed to manage assets for this employee");
        }

        // Get all existing assets for this employee
        var existingAssets = await _context.EmployeeAssets
            .Where(a => a.EmployeeId == request.EmployeeId && !a.IsDeleted)
            .ToListAsync(cancellationToken);

        // Handle explicit removal by AssetsToRemove list
        if (request.AssetsToRemove != null && request.AssetsToRemove.Count > 0)
        {
            var assetsToDelete = existingAssets
                .Where(a => request.AssetsToRemove.Contains(a.Id))
                .ToList();

            foreach (var assetToDelete in assetsToDelete)
            {
                assetToDelete.MarkAsDeleted(CurrentUser.Id ?? Guid.Empty);
            }
        }

        var resultAssets = new List<EmployeeAssetDetailsDto>();

        // Process each asset in the request
        foreach (var item in request.Assets)
        {
            if (item.AssetId.HasValue && item.AssetId.Value != Guid.Empty)
            {
                // UPDATE existing asset
                var existingAsset = existingAssets.FirstOrDefault(a => a.Id == item.AssetId.Value);
                
                if (existingAsset == null)
                {
                    // Asset not found, skip or could return error
                    continue;
                }

                // Skip if it's in the removal list
                if (request.AssetsToRemove?.Contains(item.AssetId.Value) == true)
                {
                    continue;
                }

                // Update asset properties
                existingAsset.AssetType = item.AssetType;
                existingAsset.AssetName = item.AssetName;
                existingAsset.SerialNumber = item.SerialNumber;
                existingAsset.Model = item.Model;
                existingAsset.Description = item.Description;
                existingAsset.AssignedDate = item.AssignedDate;
                existingAsset.ExpectedReturnDate = item.ExpectedReturnDate;
                existingAsset.ReturnDate = item.ReturnDate;
                existingAsset.IsReturned = item.IsReturned;
                existingAsset.Condition = item.Condition;
                existingAsset.ReturnNotes = item.ReturnNotes;
                existingAsset.Value = item.Value;

                resultAssets.Add(MapToDto(existingAsset));
            }
            else
            {
                // CREATE new asset
                var newAsset = new EmployeeAsset
                {
                    EmployeeId = request.EmployeeId,
                    AssetType = item.AssetType,
                    AssetName = item.AssetName,
                    SerialNumber = item.SerialNumber,
                    Model = item.Model,
                    Description = item.Description,
                    AssignedDate = item.AssignedDate,
                    ExpectedReturnDate = item.ExpectedReturnDate,
                    ReturnDate = item.ReturnDate,
                    IsReturned = item.IsReturned,
                    Condition = item.Condition,
                    ReturnNotes = item.ReturnNotes,
                    Value = item.Value,
                    TenantId = employee.TenantId,
                    BranchId = employee.BranchId ?? employee.TenantId
                };

                _context.EmployeeAssets.Add(newAsset);
                resultAssets.Add(MapToDto(newAsset));
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Re-query to get accurate IDs for newly created assets
        var finalAssets = await _context.EmployeeAssets
            .Where(a => a.EmployeeId == request.EmployeeId && !a.IsDeleted)
            .Select(a => new EmployeeAssetDetailsDto
            {
                Id = a.Id,
                AssetType = a.AssetType,
                AssetName = a.AssetName,
                SerialNumber = a.SerialNumber,
                Model = a.Model,
                Description = a.Description,
                AssignedDate = a.AssignedDate,
                ExpectedReturnDate = a.ExpectedReturnDate,
                ReturnDate = a.ReturnDate,
                IsReturned = a.IsReturned,
                Condition = a.Condition,
                ReturnNotes = a.ReturnNotes,
                Value = a.Value
            })
            .ToListAsync(cancellationToken);

        return new GenericResponse<List<EmployeeAssetDetailsDto>>
        {
            Success = true,
            Message = "Employee assets synced successfully",
            Data = finalAssets
        };
    }

    private static EmployeeAssetDetailsDto MapToDto(EmployeeAsset asset)
    {
        return new EmployeeAssetDetailsDto
        {
            Id = asset.Id,
            AssetType = asset.AssetType,
            AssetName = asset.AssetName,
            SerialNumber = asset.SerialNumber,
            Model = asset.Model,
            Description = asset.Description,
            AssignedDate = asset.AssignedDate,
            ExpectedReturnDate = asset.ExpectedReturnDate,
            ReturnDate = asset.ReturnDate,
            IsReturned = asset.IsReturned,
            Condition = asset.Condition,
            ReturnNotes = asset.ReturnNotes,
            Value = asset.Value
        };
    }
}
