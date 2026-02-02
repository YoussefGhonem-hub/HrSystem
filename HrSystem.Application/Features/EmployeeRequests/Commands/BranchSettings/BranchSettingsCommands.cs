using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Entities.Requests;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Commands.BranchSettings;

#region Create Branch Request Setting
public record CreateBranchRequestSettingCommand(
    Guid BranchId,
    Guid RequestTypeId,
    bool IsVisibleToEmployees,
    bool AllowEmployeesToSubmit,
    bool RequireAttachment,
    int? MaxOpenRequests,
    string? CustomInstructions
) : IRequest<ErrorOr<GenericResponse<BranchRequestSettingDetailDto>>>;

public record CreateBranchRequestSettingsCommand(
    List<CreateBranchRequestSettingDto> Settings
) : IRequest<ErrorOr<GenericResponse<List<BranchRequestSettingDetailDto>>>>;

public class CreateBranchRequestSettingCommandHandler : IRequestHandler<CreateBranchRequestSettingCommand, ErrorOr<GenericResponse<BranchRequestSettingDetailDto>>>
{
    private readonly ApplicationDbContext _context;

    public CreateBranchRequestSettingCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<BranchRequestSettingDetailDto>>> Handle(
        CreateBranchRequestSettingCommand request,
        CancellationToken cancellationToken)
    {
        var branch = await _context.Branches.FindAsync(new object[] { request.BranchId }, cancellationToken);
        if (branch == null)
            return Error.NotFound(description: "Branch not found.");

        // Check if setting already exists
        var existing = await _context.BranchRequestSettings
            .FirstOrDefaultAsync(s => s.BranchId == request.BranchId && s.RequestTypeId == request.RequestTypeId, cancellationToken);

        if (existing != null)
            return Error.Conflict(description: $"Setting for this request type already exists for this branch.");

        // Validate RequestType exists
        var requestType = await _context.RequestTypes.FindAsync(new object[] { request.RequestTypeId }, cancellationToken);
        if (requestType == null)
            return Error.Validation(description: "Invalid request type.");

        var entity = new BranchRequestSetting
        {
            BranchId = request.BranchId,
            RequestTypeId = request.RequestTypeId,
            IsVisibleToEmployees = request.IsVisibleToEmployees,
            AllowEmployeesToSubmit = request.AllowEmployeesToSubmit,
            RequireAttachment = request.RequireAttachment,
            MaxOpenRequests = request.MaxOpenRequests,
            CustomInstructions = request.CustomInstructions,
            TenantId = branch.TenantId,
            CreatedDate = DateTimeOffset.UtcNow
        };

        _context.BranchRequestSettings.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new BranchRequestSettingDetailDto
        {
            Id = entity.Id,
            BranchId = entity.BranchId ?? Guid.Empty,
            BranchName = branch.NameEn,
            RequestTypeId = entity.RequestTypeId,
            RequestTypeName = requestType.Code,
            IsVisibleToEmployees = entity.IsVisibleToEmployees,
            AllowEmployeesToSubmit = entity.AllowEmployeesToSubmit,
            RequireAttachment = entity.RequireAttachment,
            MaxOpenRequests = entity.MaxOpenRequests,
            CustomInstructions = entity.CustomInstructions,
            CreatedDate = entity.CreatedDate,
            ModifiedDate = entity.ModifiedDate
        };

        return GenericResponse<BranchRequestSettingDetailDto>.SuccessResult(dto, "Branch request setting created successfully");
    }
}

public class CreateBranchRequestSettingsCommandHandler : IRequestHandler<CreateBranchRequestSettingsCommand, ErrorOr<GenericResponse<List<BranchRequestSettingDetailDto>>>>
{
    private readonly ApplicationDbContext _context;

    public CreateBranchRequestSettingsCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<List<BranchRequestSettingDetailDto>>>> Handle(
        CreateBranchRequestSettingsCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Settings == null || !request.Settings.Any())
            return Error.Validation(description: "At least one setting is required.");

        var createdSettings = new List<BranchRequestSettingDetailDto>();
        var errors = new List<string>();

        // Get all unique branch IDs and validate them
        var branchIds = request.Settings.Select(s => s.BranchId).Distinct().ToList();
        var branches = await _context.Branches
            .Where(b => branchIds.Contains(b.Id))
            .ToDictionaryAsync(b => b.Id, cancellationToken);

        // Get all unique request type IDs and validate them
        var requestTypeIds = request.Settings.Select(s => s.RequestTypeId).Distinct().ToList();
        var requestTypes = await _context.RequestTypes
            .Where(rt => requestTypeIds.Contains(rt.Id))
            .ToDictionaryAsync(rt => rt.Id, cancellationToken);

        // Get existing settings to check for duplicates
        var existingSettings = await _context.BranchRequestSettings
            .Where(s => branchIds.Contains(s.BranchId ?? Guid.Empty))
            .Select(s => new { s.BranchId, s.RequestTypeId })
            .ToListAsync(cancellationToken);

        var existingSet = existingSettings
            .Select(s => (s.BranchId, s.RequestTypeId))
            .ToHashSet();

        var entitiesToAdd = new List<BranchRequestSetting>();

        foreach (var dto in request.Settings)
        {
            // Validate branch
            if (!branches.TryGetValue(dto.BranchId, out var branch))
            {
                errors.Add($"Branch with ID {dto.BranchId} not found.");
                continue;
            }

            // Validate request type
            if (!requestTypes.TryGetValue(dto.RequestTypeId, out var requestType))
            {
                errors.Add($"Request type with ID {dto.RequestTypeId} not found.");
                continue;
            }

            // Check for duplicates
            if (existingSet.Contains((dto.BranchId, dto.RequestTypeId)))
            {
                errors.Add($"Setting for branch '{branch.NameEn}' and request type '{requestType.Code}' already exists.");
                continue;
            }

            // Add to set to prevent duplicates within the same request
            if (!existingSet.Add((dto.BranchId, dto.RequestTypeId)))
            {
                errors.Add($"Duplicate setting for branch '{branch.NameEn}' and request type '{requestType.Code}' in request.");
                continue;
            }

            var entity = new BranchRequestSetting
            {
                BranchId = dto.BranchId,
                RequestTypeId = dto.RequestTypeId,
                IsVisibleToEmployees = dto.IsVisibleToEmployees,
                AllowEmployeesToSubmit = dto.AllowEmployeesToSubmit,
                RequireAttachment = dto.RequireAttachment,
                MaxOpenRequests = dto.MaxOpenRequests,
                CustomInstructions = dto.CustomInstructions,
                TenantId = branch.TenantId,
                CreatedDate = DateTimeOffset.UtcNow
            };

            entitiesToAdd.Add(entity);
        }

        if (entitiesToAdd.Any())
        {
            _context.BranchRequestSettings.AddRange(entitiesToAdd);
            await _context.SaveChangesAsync(cancellationToken);

            foreach (var entity in entitiesToAdd)
            {
                branches.TryGetValue(entity.BranchId ?? Guid.Empty, out var branch);
                requestTypes.TryGetValue(entity.RequestTypeId, out var requestType);

                createdSettings.Add(new BranchRequestSettingDetailDto
                {
                    Id = entity.Id,
                    BranchId = entity.BranchId ?? Guid.Empty,
                    BranchName = branch?.NameEn,
                    RequestTypeId = entity.RequestTypeId,
                    RequestTypeName = requestType?.Code ?? "",
                    IsVisibleToEmployees = entity.IsVisibleToEmployees,
                    AllowEmployeesToSubmit = entity.AllowEmployeesToSubmit,
                    RequireAttachment = entity.RequireAttachment,
                    MaxOpenRequests = entity.MaxOpenRequests,
                    CustomInstructions = entity.CustomInstructions,
                    CreatedDate = entity.CreatedDate,
                    ModifiedDate = entity.ModifiedDate
                });
            }
        }

        if (errors.Any() && !createdSettings.Any())
            return Error.Validation(description: string.Join(" ", errors));

        var message = $"{createdSettings.Count} branch request setting(s) created successfully.";
        if (errors.Any())
            message += $" {errors.Count} setting(s) skipped: {string.Join(" ", errors)}";

        return GenericResponse<List<BranchRequestSettingDetailDto>>.SuccessResult(createdSettings, message);
    }
}
#endregion

#region Update Branch Request Setting
public record UpdateBranchRequestSettingCommand(
    Guid Id,
    bool IsVisibleToEmployees,
    bool AllowEmployeesToSubmit,
    bool RequireAttachment,
    int? MaxOpenRequests,
    string? CustomInstructions
) : IRequest<ErrorOr<GenericResponse<BranchRequestSettingDetailDto>>>;

public class UpdateBranchRequestSettingCommandHandler : IRequestHandler<UpdateBranchRequestSettingCommand, ErrorOr<GenericResponse<BranchRequestSettingDetailDto>>>
{
    private readonly ApplicationDbContext _context;

    public UpdateBranchRequestSettingCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<BranchRequestSettingDetailDto>>> Handle(
        UpdateBranchRequestSettingCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await _context.BranchRequestSettings
            .Include(s => s.Branch)
            .Include(s => s.RequestTypeRef)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (entity == null)
            return Error.NotFound(description: "Branch request setting not found.");

        entity.IsVisibleToEmployees = request.IsVisibleToEmployees;
        entity.AllowEmployeesToSubmit = request.AllowEmployeesToSubmit;
        entity.RequireAttachment = request.RequireAttachment;
        entity.MaxOpenRequests = request.MaxOpenRequests;
        entity.CustomInstructions = request.CustomInstructions;
        entity.ModifiedDate = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var dto = new BranchRequestSettingDetailDto
        {
            Id = entity.Id,
            BranchId = entity.BranchId ?? Guid.Empty,
            BranchName = entity.Branch?.NameEn,
            RequestTypeId = entity.RequestTypeId,
            RequestTypeName = entity.RequestTypeRef?.Code ?? "",
            IsVisibleToEmployees = entity.IsVisibleToEmployees,
            AllowEmployeesToSubmit = entity.AllowEmployeesToSubmit,
            RequireAttachment = entity.RequireAttachment,
            MaxOpenRequests = entity.MaxOpenRequests,
            CustomInstructions = entity.CustomInstructions,
            CreatedDate = entity.CreatedDate,
            ModifiedDate = entity.ModifiedDate
        };

        return GenericResponse<BranchRequestSettingDetailDto>.SuccessResult(dto, "Branch request setting updated successfully");
    }
}
#endregion

#region Delete Branch Request Setting
public record DeleteBranchRequestSettingCommand(Guid Id) : IRequest<ErrorOr<GenericResponse>>;

public class DeleteBranchRequestSettingCommandHandler : IRequestHandler<DeleteBranchRequestSettingCommand, ErrorOr<GenericResponse>>
{
    private readonly ApplicationDbContext _context;

    public DeleteBranchRequestSettingCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse>> Handle(
        DeleteBranchRequestSettingCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await _context.BranchRequestSettings.FindAsync(new object[] { request.Id }, cancellationToken);
        if (entity == null)
            return Error.NotFound(description: "Branch request setting not found.");

        _context.BranchRequestSettings.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return GenericResponse.SuccessResult("Branch request setting deleted successfully");
    }
}
#endregion

#region Initialize Branch Settings (Create all 6 request types for a branch)
public record InitializeBranchSettingsCommand(
    Guid BranchId,
    bool EnableAllRequestTypes = true
) : IRequest<ErrorOr<GenericResponse<List<BranchRequestSettingDetailDto>>>>;

public class InitializeBranchSettingsCommandHandler : IRequestHandler<InitializeBranchSettingsCommand, ErrorOr<GenericResponse<List<BranchRequestSettingDetailDto>>>>
{
    private readonly ApplicationDbContext _context;

    public InitializeBranchSettingsCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<List<BranchRequestSettingDetailDto>>>> Handle(
        InitializeBranchSettingsCommand request,
        CancellationToken cancellationToken)
    {
        var branch = await _context.Branches.FindAsync(new object[] { request.BranchId }, cancellationToken);
        if (branch == null)
            return Error.NotFound(description: "Branch not found.");

        // Get all active request types from master table
        var allRequestTypes = await _context.RequestTypes
            .Where(rt => rt.IsActive)
            .ToListAsync(cancellationToken);

        // Get existing settings for this branch
        var existingTypeIds = await _context.BranchRequestSettings
            .Where(s => s.BranchId == request.BranchId)
            .Select(s => s.RequestTypeId)
            .ToListAsync(cancellationToken);

        var newSettings = new List<BranchRequestSetting>();

        foreach (var requestType in allRequestTypes)
        {
            if (existingTypeIds.Contains(requestType.Id))
                continue;

            var setting = new BranchRequestSetting
            {
                BranchId = request.BranchId,
                RequestTypeId = requestType.Id,
                IsVisibleToEmployees = request.EnableAllRequestTypes,
                AllowEmployeesToSubmit = request.EnableAllRequestTypes,
                RequireAttachment = false,
                MaxOpenRequests = null,
                CustomInstructions = null,
                TenantId = branch.TenantId,
                CreatedDate = DateTimeOffset.UtcNow
            };

            newSettings.Add(setting);
        }

        if (newSettings.Any())
        {
            _context.BranchRequestSettings.AddRange(newSettings);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // Fetch all settings (existing + new)
        var allSettings = await _context.BranchRequestSettings
            .Include(s => s.Branch)
            .Include(s => s.RequestTypeRef)
            .Where(s => s.BranchId == request.BranchId)
            .OrderBy(s => s.RequestTypeRef != null ? s.RequestTypeRef.SortOrder : 0)
            .ToListAsync(cancellationToken);

        var dtos = allSettings.Select(e => new BranchRequestSettingDetailDto
        {
            Id = e.Id,
            BranchId = e.BranchId ?? Guid.Empty,
            BranchName = e.Branch?.NameEn,
            RequestTypeId = e.RequestTypeId,
            RequestTypeName = e.RequestTypeRef?.Code ?? "",
            IsVisibleToEmployees = e.IsVisibleToEmployees,
            AllowEmployeesToSubmit = e.AllowEmployeesToSubmit,
            RequireAttachment = e.RequireAttachment,
            MaxOpenRequests = e.MaxOpenRequests,
            CustomInstructions = e.CustomInstructions,
            CreatedDate = e.CreatedDate,
            ModifiedDate = e.ModifiedDate
        }).ToList();

        return GenericResponse<List<BranchRequestSettingDetailDto>>.SuccessResult(
            dtos,
            $"Branch settings initialized. {newSettings.Count} new settings created.");
    }
}
#endregion
