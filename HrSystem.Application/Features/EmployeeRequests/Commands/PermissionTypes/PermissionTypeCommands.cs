using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Entities.Requests;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;

namespace HrSystem.Application.Features.EmployeeRequests.Commands.PermissionTypes;

#region Create PermissionType
public record CreatePermissionTypeCommand(
    string NameAr,
    string NameEn,
    string? Description,
    decimal? MaxHoursPerRequest,
    decimal? MaxHoursPerMonth,
    bool DeductsFromLeave,
    decimal? HoursPerLeaveDay,
    bool RequiresAttachment,
    bool RequiresManagerApproval,
    bool IsActive,
    int SortOrder
) : IRequest<ErrorOr<GenericResponse<PermissionTypeDetailDto>>>;

public class CreatePermissionTypeCommandHandler : IRequestHandler<CreatePermissionTypeCommand, ErrorOr<GenericResponse<PermissionTypeDetailDto>>>
{
    private readonly ApplicationDbContext _context;

    public CreatePermissionTypeCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<PermissionTypeDetailDto>>> Handle(
        CreatePermissionTypeCommand request,
        CancellationToken cancellationToken)
    {
        var entity = new PermissionType
        {
            NameAr = request.NameAr,
            NameEn = request.NameEn,
            Description = request.Description,
            MaxHoursPerRequest = request.MaxHoursPerRequest,
            MaxHoursPerMonth = request.MaxHoursPerMonth,
            DeductsFromLeave = request.DeductsFromLeave,
            HoursPerLeaveDay = request.HoursPerLeaveDay,
            RequiresAttachment = request.RequiresAttachment,
            RequiresManagerApproval = request.RequiresManagerApproval,
            IsActive = request.IsActive,
            SortOrder = request.SortOrder,
            TenantId = CurrentUser.OrganizationId ?? Guid.Empty,
            CreatedDate = DateTimeOffset.UtcNow
        };

        _context.PermissionTypes.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new PermissionTypeDetailDto
        {
            Id = entity.Id,
            NameAr = entity.NameAr,
            NameEn = entity.NameEn,
            Description = entity.Description,
            MaxHoursPerRequest = entity.MaxHoursPerRequest,
            MaxHoursPerMonth = entity.MaxHoursPerMonth,
            DeductsFromLeave = entity.DeductsFromLeave,
            HoursPerLeaveDay = entity.HoursPerLeaveDay,
            RequiresAttachment = entity.RequiresAttachment,
            RequiresManagerApproval = entity.RequiresManagerApproval,
            IsActive = entity.IsActive,
            SortOrder = entity.SortOrder,
            CreatedDate = entity.CreatedDate,
            ModifiedDate = entity.ModifiedDate
        };

        return GenericResponse<PermissionTypeDetailDto>.SuccessResult(dto, "Permission type created successfully");
    }
}
#endregion

#region Update PermissionType
public record UpdatePermissionTypeCommand(
    Guid Id,
    string NameAr,
    string NameEn,
    string? Description,
    decimal? MaxHoursPerRequest,
    decimal? MaxHoursPerMonth,
    bool DeductsFromLeave,
    decimal? HoursPerLeaveDay,
    bool RequiresAttachment,
    bool RequiresManagerApproval,
    bool IsActive,
    int SortOrder
) : IRequest<ErrorOr<GenericResponse<PermissionTypeDetailDto>>>;

public class UpdatePermissionTypeCommandHandler : IRequestHandler<UpdatePermissionTypeCommand, ErrorOr<GenericResponse<PermissionTypeDetailDto>>>
{
    private readonly ApplicationDbContext _context;

    public UpdatePermissionTypeCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<PermissionTypeDetailDto>>> Handle(
        UpdatePermissionTypeCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await _context.PermissionTypes.FindAsync(new object[] { request.Id }, cancellationToken);
        if (entity == null)
            return Error.NotFound(description: "Permission type not found.");

        entity.NameAr = request.NameAr;
        entity.NameEn = request.NameEn;
        entity.Description = request.Description;
        entity.MaxHoursPerRequest = request.MaxHoursPerRequest;
        entity.MaxHoursPerMonth = request.MaxHoursPerMonth;
        entity.DeductsFromLeave = request.DeductsFromLeave;
        entity.HoursPerLeaveDay = request.HoursPerLeaveDay;
        entity.RequiresAttachment = request.RequiresAttachment;
        entity.RequiresManagerApproval = request.RequiresManagerApproval;
        entity.IsActive = request.IsActive;
        entity.SortOrder = request.SortOrder;
        entity.ModifiedDate = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var dto = new PermissionTypeDetailDto
        {
            Id = entity.Id,
            NameAr = entity.NameAr,
            NameEn = entity.NameEn,
            Description = entity.Description,
            MaxHoursPerRequest = entity.MaxHoursPerRequest,
            MaxHoursPerMonth = entity.MaxHoursPerMonth,
            DeductsFromLeave = entity.DeductsFromLeave,
            HoursPerLeaveDay = entity.HoursPerLeaveDay,
            RequiresAttachment = entity.RequiresAttachment,
            RequiresManagerApproval = entity.RequiresManagerApproval,
            IsActive = entity.IsActive,
            SortOrder = entity.SortOrder,
            CreatedDate = entity.CreatedDate,
            ModifiedDate = entity.ModifiedDate
        };

        return GenericResponse<PermissionTypeDetailDto>.SuccessResult(dto, "Permission type updated successfully");
    }
}
#endregion

#region Delete PermissionType
public record DeletePermissionTypeCommand(Guid Id) : IRequest<ErrorOr<GenericResponse>>;

public class DeletePermissionTypeCommandHandler : IRequestHandler<DeletePermissionTypeCommand, ErrorOr<GenericResponse>>
{
    private readonly ApplicationDbContext _context;

    public DeletePermissionTypeCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse>> Handle(
        DeletePermissionTypeCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await _context.PermissionTypes.FindAsync(new object[] { request.Id }, cancellationToken);
        if (entity == null)
            return Error.NotFound(description: "Permission type not found.");

        // Soft delete by setting IsActive = false
        entity.IsActive = false;
        entity.ModifiedDate = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return GenericResponse.SuccessResult("Permission type deleted successfully");
    }
}
#endregion
