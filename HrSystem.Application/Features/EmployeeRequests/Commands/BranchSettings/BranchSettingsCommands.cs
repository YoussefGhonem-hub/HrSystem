using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Entities.Requests;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Commands.BranchSettings;

#region Create Branch Request Setting
public record CreateBranchRequestSettingCommand(
    Guid BranchId,
    EmployeeRequestType RequestType,
    bool IsVisibleToEmployees,
    bool AllowEmployeesToSubmit,
    bool RequireAttachment,
    int? MaxOpenRequests,
    string? CustomInstructions
) : IRequest<ErrorOr<GenericResponse<BranchRequestSettingDetailDto>>>;

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
            .FirstOrDefaultAsync(s => s.BranchId == request.BranchId && s.RequestType == request.RequestType, cancellationToken);

        if (existing != null)
            return Error.Conflict(description: $"Setting for request type '{request.RequestType}' already exists for this branch.");

        var entity = new BranchRequestSetting
        {
            BranchId = request.BranchId,
            RequestType = request.RequestType,
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
            RequestType = entity.RequestType,
            RequestTypeName = entity.RequestType.ToString(),
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
            RequestType = entity.RequestType,
            RequestTypeName = entity.RequestType.ToString(),
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

        // Get existing settings for this branch
        var existingTypes = await _context.BranchRequestSettings
            .Where(s => s.BranchId == request.BranchId)
            .Select(s => s.RequestType)
            .ToListAsync(cancellationToken);

        var allRequestTypes = Enum.GetValues<EmployeeRequestType>();
        var newSettings = new List<BranchRequestSetting>();

        foreach (var requestType in allRequestTypes)
        {
            if (existingTypes.Contains(requestType))
                continue;

            var setting = new BranchRequestSetting
            {
                BranchId = request.BranchId,
                RequestType = requestType,
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
            .Where(s => s.BranchId == request.BranchId)
            .OrderBy(s => s.RequestType)
            .ToListAsync(cancellationToken);

        var dtos = allSettings.Select(e => new BranchRequestSettingDetailDto
        {
            Id = e.Id,
            BranchId = e.BranchId ?? Guid.Empty,
            BranchName = e.Branch?.NameEn,
            RequestType = e.RequestType,
            RequestTypeName = e.RequestType.ToString(),
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
