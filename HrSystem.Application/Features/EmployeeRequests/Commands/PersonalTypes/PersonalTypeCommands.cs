using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Entities.Requests;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.EmployeeRequests.Commands.PersonalTypes;

#region Create PersonalType
public record CreatePersonalTypeCommand(
    string NameAr,
    string NameEn,
    string? Description,
    bool RequiresManagerApproval,
    bool RequireAttachment,
    bool IsActive,
    int SortOrder
) : IRequest<ErrorOr<GenericResponse<PersonalTypeDetailDto>>>;

public class CreatePersonalTypeCommandHandler : IRequestHandler<CreatePersonalTypeCommand, ErrorOr<GenericResponse<PersonalTypeDetailDto>>>
{
    private readonly ApplicationDbContext _context;

    public CreatePersonalTypeCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<PersonalTypeDetailDto>>> Handle(
        CreatePersonalTypeCommand request,
        CancellationToken cancellationToken)
    {
        var entity = new PersonalType
        {
            NameAr = request.NameAr,
            NameEn = request.NameEn,
            Description = request.Description,
            RequiresManagerApproval = request.RequiresManagerApproval,
            RequireAttachment = request.RequireAttachment,
            IsActive = request.IsActive,
            SortOrder = request.SortOrder,
            CreatedDate = DateTimeOffset.UtcNow
        };

        _context.PersonalTypes.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new PersonalTypeDetailDto
        {
            Id = entity.Id,
            NameAr = entity.NameAr,
            NameEn = entity.NameEn,
            Description = entity.Description,
            RequiresManagerApproval = entity.RequiresManagerApproval,
            RequireAttachment = entity.RequireAttachment,
            IsActive = entity.IsActive,
            SortOrder = entity.SortOrder,
            CreatedDate = entity.CreatedDate,
            ModifiedDate = entity.ModifiedDate
        };

        return GenericResponse<PersonalTypeDetailDto>.SuccessResult(dto, "Personal type created successfully");
    }
}
#endregion

#region Update PersonalType
public record UpdatePersonalTypeCommand(
    Guid Id,
    string NameAr,
    string NameEn,
    string? Description,
    bool RequiresManagerApproval,
    bool RequireAttachment,
    bool IsActive,
    int SortOrder
) : IRequest<ErrorOr<GenericResponse<PersonalTypeDetailDto>>>;

public class UpdatePersonalTypeCommandHandler : IRequestHandler<UpdatePersonalTypeCommand, ErrorOr<GenericResponse<PersonalTypeDetailDto>>>
{
    private readonly ApplicationDbContext _context;

    public UpdatePersonalTypeCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<PersonalTypeDetailDto>>> Handle(
        UpdatePersonalTypeCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await _context.PersonalTypes.FindAsync(new object[] { request.Id }, cancellationToken);
        if (entity == null)
            return Error.NotFound(description: "Personal type not found.");

        entity.NameAr = request.NameAr;
        entity.NameEn = request.NameEn;
        entity.Description = request.Description;
        entity.RequiresManagerApproval = request.RequiresManagerApproval;
        entity.RequireAttachment = request.RequireAttachment;
        entity.IsActive = request.IsActive;
        entity.SortOrder = request.SortOrder;
        entity.ModifiedDate = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var dto = new PersonalTypeDetailDto
        {
            Id = entity.Id,
            NameAr = entity.NameAr,
            NameEn = entity.NameEn,
            Description = entity.Description,
            RequiresManagerApproval = entity.RequiresManagerApproval,
            RequireAttachment = entity.RequireAttachment,
            IsActive = entity.IsActive,
            SortOrder = entity.SortOrder,
            CreatedDate = entity.CreatedDate,
            ModifiedDate = entity.ModifiedDate
        };

        return GenericResponse<PersonalTypeDetailDto>.SuccessResult(dto, "Personal type updated successfully");
    }
}
#endregion

#region Delete PersonalType
public record DeletePersonalTypeCommand(Guid Id) : IRequest<ErrorOr<GenericResponse>>;

public class DeletePersonalTypeCommandHandler : IRequestHandler<DeletePersonalTypeCommand, ErrorOr<GenericResponse>>
{
    private readonly ApplicationDbContext _context;

    public DeletePersonalTypeCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse>> Handle(
        DeletePersonalTypeCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await _context.PersonalTypes.FindAsync(new object[] { request.Id }, cancellationToken);
        if (entity == null)
            return Error.NotFound(description: "Personal type not found.");

        entity.IsActive = false;
        entity.ModifiedDate = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return GenericResponse.SuccessResult("Personal type deleted successfully");
    }
}
#endregion
