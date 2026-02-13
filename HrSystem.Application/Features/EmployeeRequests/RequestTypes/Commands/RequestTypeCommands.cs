using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Entities.Requests;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.RequestTypes.Commands;

public record CreateRequestTypeCommand(CreateRequestTypeMasterDto Payload) : IRequest<ErrorOr<GenericResponse<RequestTypeDto>>>;
public record UpdateRequestTypeCommand(UpdateRequestTypeMasterDto Payload) : IRequest<ErrorOr<GenericResponse<RequestTypeDto>>>;
public record DeleteRequestTypeCommand(Guid Id) : IRequest<ErrorOr<GenericResponse>>;

public class CreateRequestTypeCommandHandler : IRequestHandler<CreateRequestTypeCommand, ErrorOr<GenericResponse<RequestTypeDto>>>
{
    private readonly ApplicationDbContext _context;

    public CreateRequestTypeCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<RequestTypeDto>>> Handle(CreateRequestTypeCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Payload;

        var code = dto.Code.Trim();

        var codeExists = await _context.RequestTypes
            .AnyAsync(r => !r.IsDeleted && r.Code == code, cancellationToken);

        if (codeExists)
        {
            return Error.Conflict(description: "Code already exists");
        }

        var entity = new RequestType
        {
            Code = code,
            NameAr = dto.NameAr,
            NameEn = dto.NameEn,
            Description = dto.Description,
            IsActive = dto.IsActive,
            SortOrder = dto.SortOrder,
            RequireAttachment = dto.RequireAttachment
        };

        entity.MarkAsCreated(CurrentUser.Id ?? Guid.Empty);

        _context.RequestTypes.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return GenericResponse<RequestTypeDto>.SuccessResult(RequestTypeMapper.ToDto(entity), "Request type created");
    }
}

public class UpdateRequestTypeCommandHandler : IRequestHandler<UpdateRequestTypeCommand, ErrorOr<GenericResponse<RequestTypeDto>>>
{
    private readonly ApplicationDbContext _context;

    public UpdateRequestTypeCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<RequestTypeDto>>> Handle(UpdateRequestTypeCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Payload;

        var entity = await _context.RequestTypes
            .FirstOrDefaultAsync(r => r.Id == dto.Id && !r.IsDeleted, cancellationToken);

        if (entity == null)
            return Error.NotFound(description: "Request type not found");

        var code = dto.Code.Trim();

        var codeExists = await _context.RequestTypes
            .AnyAsync(r => r.Id != dto.Id && !r.IsDeleted && r.Code == code, cancellationToken);

        if (codeExists)
        {
            return Error.Conflict(description: "Code already exists");
        }

        entity.Code = code;
        entity.NameAr = dto.NameAr;
        entity.NameEn = dto.NameEn;
        entity.Description = dto.Description;
        entity.IsActive = dto.IsActive;
        entity.SortOrder = dto.SortOrder;
        entity.RequireAttachment = dto.RequireAttachment;
        entity.MarkAsModified(CurrentUser.Id ?? Guid.Empty);

        await _context.SaveChangesAsync(cancellationToken);

        return GenericResponse<RequestTypeDto>.SuccessResult(RequestTypeMapper.ToDto(entity), "Request type updated");
    }
}

public class DeleteRequestTypeCommandHandler : IRequestHandler<DeleteRequestTypeCommand, ErrorOr<GenericResponse>>
{
    private readonly ApplicationDbContext _context;

    public DeleteRequestTypeCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse>> Handle(DeleteRequestTypeCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.RequestTypes
            .FirstOrDefaultAsync(r => r.Id == request.Id && !r.IsDeleted, cancellationToken);

        if (entity == null)
            return Error.NotFound(description: "Request type not found");

        entity.MarkAsDeleted(CurrentUser.Id ?? Guid.Empty);
        entity.IsActive = false;

        await _context.SaveChangesAsync(cancellationToken);

        return GenericResponse.SuccessResult("Request type deleted");
    }
}

internal static class RequestTypeMapper
{
    public static RequestTypeDto ToDto(RequestType entity) => new()
    {
        Id = entity.Id,
        Code = entity.Code,
        NameAr = entity.NameAr,
        NameEn = entity.NameEn,
        Description = entity.Description,
        IsActive = entity.IsActive,
        SortOrder = entity.SortOrder,
        RequireAttachment = entity.RequireAttachment,
        CreatedDate = entity.CreatedDate,
        ModifiedDate = entity.ModifiedDate
    };
}
