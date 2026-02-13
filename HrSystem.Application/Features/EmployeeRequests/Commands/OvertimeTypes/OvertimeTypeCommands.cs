using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Entities.Requests;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.EmployeeRequests.Commands.OvertimeTypes;

#region Create OvertimeType
public record CreateOvertimeTypeCommand(
    string NameAr,
    string NameEn,
    string? Description,
    decimal DefaultMultiplier,
    bool RequiresManagerApproval,
    bool IsActive,
    int SortOrder
) : IRequest<ErrorOr<GenericResponse<OvertimeTypeDetailDto>>>;

public class CreateOvertimeTypeCommandHandler : IRequestHandler<CreateOvertimeTypeCommand, ErrorOr<GenericResponse<OvertimeTypeDetailDto>>>
{
    private readonly ApplicationDbContext _context;

    public CreateOvertimeTypeCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<OvertimeTypeDetailDto>>> Handle(
        CreateOvertimeTypeCommand request,
        CancellationToken cancellationToken)
    {
        var entity = new OvertimeType
        {
            NameAr = request.NameAr,
            NameEn = request.NameEn,
            Description = request.Description,
            DefaultMultiplier = request.DefaultMultiplier,
            RequiresManagerApproval = request.RequiresManagerApproval,
            IsActive = request.IsActive,
            SortOrder = request.SortOrder,
            CreatedDate = DateTimeOffset.UtcNow
        };

        _context.OvertimeTypes.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new OvertimeTypeDetailDto
        {
            Id = entity.Id,
            NameAr = entity.NameAr,
            NameEn = entity.NameEn,
            Description = entity.Description,
            DefaultMultiplier = entity.DefaultMultiplier,
            RequiresManagerApproval = entity.RequiresManagerApproval,
            IsActive = entity.IsActive,
            SortOrder = entity.SortOrder,
            CreatedDate = entity.CreatedDate,
            ModifiedDate = entity.ModifiedDate
        };

        return GenericResponse<OvertimeTypeDetailDto>.SuccessResult(dto, "Overtime type created successfully");
    }
}
#endregion

#region Update OvertimeType
public record UpdateOvertimeTypeCommand(
    Guid Id,
    string NameAr,
    string NameEn,
    string? Description,
    decimal DefaultMultiplier,
    bool RequiresManagerApproval,
    bool IsActive,
    int SortOrder
) : IRequest<ErrorOr<GenericResponse<OvertimeTypeDetailDto>>>;

public class UpdateOvertimeTypeCommandHandler : IRequestHandler<UpdateOvertimeTypeCommand, ErrorOr<GenericResponse<OvertimeTypeDetailDto>>>
{
    private readonly ApplicationDbContext _context;

    public UpdateOvertimeTypeCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<OvertimeTypeDetailDto>>> Handle(
        UpdateOvertimeTypeCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await _context.OvertimeTypes.FindAsync(new object[] { request.Id }, cancellationToken);
        if (entity == null)
            return Error.NotFound(description: "Overtime type not found.");

        entity.NameAr = request.NameAr;
        entity.NameEn = request.NameEn;
        entity.Description = request.Description;
        entity.DefaultMultiplier = request.DefaultMultiplier;
        entity.RequiresManagerApproval = request.RequiresManagerApproval;
        entity.IsActive = request.IsActive;
        entity.SortOrder = request.SortOrder;
        entity.ModifiedDate = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var dto = new OvertimeTypeDetailDto
        {
            Id = entity.Id,
            NameAr = entity.NameAr,
            NameEn = entity.NameEn,
            Description = entity.Description,
            DefaultMultiplier = entity.DefaultMultiplier,
            RequiresManagerApproval = entity.RequiresManagerApproval,
            IsActive = entity.IsActive,
            SortOrder = entity.SortOrder,
            CreatedDate = entity.CreatedDate,
            ModifiedDate = entity.ModifiedDate
        };

        return GenericResponse<OvertimeTypeDetailDto>.SuccessResult(dto, "Overtime type updated successfully");
    }
}
#endregion

#region Delete OvertimeType
public record DeleteOvertimeTypeCommand(Guid Id) : IRequest<ErrorOr<GenericResponse>>;

public class DeleteOvertimeTypeCommandHandler : IRequestHandler<DeleteOvertimeTypeCommand, ErrorOr<GenericResponse>>
{
    private readonly ApplicationDbContext _context;

    public DeleteOvertimeTypeCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse>> Handle(
        DeleteOvertimeTypeCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await _context.OvertimeTypes.FindAsync(new object[] { request.Id }, cancellationToken);
        if (entity == null)
            return Error.NotFound(description: "Overtime type not found.");

        // Soft delete by setting IsActive = false
        entity.IsActive = false;
        entity.ModifiedDate = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return GenericResponse.SuccessResult("Overtime type deleted successfully");
    }
}
#endregion
