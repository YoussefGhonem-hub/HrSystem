using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Entities.Requests;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;

namespace HrSystem.Application.Features.EmployeeRequests.Commands.MiscellaneousTypes;

#region Create MiscellaneousType
public record CreateMiscellaneousTypeCommand(
    string NameAr,
    string NameEn,
    string? Description,
    bool RequiresAttachment,
    bool RequiresManagerApproval,
    bool IsActive,
    int SortOrder
) : IRequest<ErrorOr<GenericResponse<MiscellaneousTypeDetailDto>>>;

public class CreateMiscellaneousTypeCommandHandler : IRequestHandler<CreateMiscellaneousTypeCommand, ErrorOr<GenericResponse<MiscellaneousTypeDetailDto>>>
{
    private readonly ApplicationDbContext _context;

    public CreateMiscellaneousTypeCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<MiscellaneousTypeDetailDto>>> Handle(
        CreateMiscellaneousTypeCommand request,
        CancellationToken cancellationToken)
    {
        var entity = new MiscellaneousType
        {
            NameAr = request.NameAr,
            NameEn = request.NameEn,
            Description = request.Description,
            RequiresAttachment = request.RequiresAttachment,
            RequiresManagerApproval = request.RequiresManagerApproval,
            IsActive = request.IsActive,
            SortOrder = request.SortOrder,
            TenantId = CurrentUser.OrganizationId ?? Guid.Empty,
            CreatedDate = DateTimeOffset.UtcNow
        };

        _context.MiscellaneousTypes.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new MiscellaneousTypeDetailDto
        {
            Id = entity.Id,
            NameAr = entity.NameAr,
            NameEn = entity.NameEn,
            Description = entity.Description,
            RequiresAttachment = entity.RequiresAttachment,
            RequiresManagerApproval = entity.RequiresManagerApproval,
            IsActive = entity.IsActive,
            SortOrder = entity.SortOrder,
            CreatedDate = entity.CreatedDate,
            ModifiedDate = entity.ModifiedDate
        };

        return GenericResponse<MiscellaneousTypeDetailDto>.SuccessResult(dto, "Miscellaneous type created successfully");
    }
}
#endregion

#region Update MiscellaneousType
public record UpdateMiscellaneousTypeCommand(
    Guid Id,
    string NameAr,
    string NameEn,
    string? Description,
    bool RequiresAttachment,
    bool RequiresManagerApproval,
    bool IsActive,
    int SortOrder
) : IRequest<ErrorOr<GenericResponse<MiscellaneousTypeDetailDto>>>;

public class UpdateMiscellaneousTypeCommandHandler : IRequestHandler<UpdateMiscellaneousTypeCommand, ErrorOr<GenericResponse<MiscellaneousTypeDetailDto>>>
{
    private readonly ApplicationDbContext _context;

    public UpdateMiscellaneousTypeCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<MiscellaneousTypeDetailDto>>> Handle(
        UpdateMiscellaneousTypeCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await _context.MiscellaneousTypes.FindAsync(new object[] { request.Id }, cancellationToken);
        if (entity == null)
            return Error.NotFound(description: "Miscellaneous type not found.");

        entity.NameAr = request.NameAr;
        entity.NameEn = request.NameEn;
        entity.Description = request.Description;
        entity.RequiresAttachment = request.RequiresAttachment;
        entity.RequiresManagerApproval = request.RequiresManagerApproval;
        entity.IsActive = request.IsActive;
        entity.SortOrder = request.SortOrder;
        entity.ModifiedDate = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var dto = new MiscellaneousTypeDetailDto
        {
            Id = entity.Id,
            NameAr = entity.NameAr,
            NameEn = entity.NameEn,
            Description = entity.Description,
            RequiresAttachment = entity.RequiresAttachment,
            RequiresManagerApproval = entity.RequiresManagerApproval,
            IsActive = entity.IsActive,
            SortOrder = entity.SortOrder,
            CreatedDate = entity.CreatedDate,
            ModifiedDate = entity.ModifiedDate
        };

        return GenericResponse<MiscellaneousTypeDetailDto>.SuccessResult(dto, "Miscellaneous type updated successfully");
    }
}
#endregion

#region Delete MiscellaneousType
public record DeleteMiscellaneousTypeCommand(Guid Id) : IRequest<ErrorOr<GenericResponse>>;

public class DeleteMiscellaneousTypeCommandHandler : IRequestHandler<DeleteMiscellaneousTypeCommand, ErrorOr<GenericResponse>>
{
    private readonly ApplicationDbContext _context;

    public DeleteMiscellaneousTypeCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse>> Handle(
        DeleteMiscellaneousTypeCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await _context.MiscellaneousTypes.FindAsync(new object[] { request.Id }, cancellationToken);
        if (entity == null)
            return Error.NotFound(description: "Miscellaneous type not found.");

        entity.IsActive = false;
        entity.ModifiedDate = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return GenericResponse.SuccessResult("Miscellaneous type deleted successfully");
    }
}
#endregion
