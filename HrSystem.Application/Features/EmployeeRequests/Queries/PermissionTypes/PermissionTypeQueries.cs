using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.PermissionTypes;

#region Get All PermissionTypes
public record GetPermissionTypesQuery(
    bool? IsActive = null,
    string? SearchTerm = null
) : IRequest<ErrorOr<GenericResponse<List<PermissionTypeDetailDto>>>>;

public class GetPermissionTypesQueryHandler : IRequestHandler<GetPermissionTypesQuery, ErrorOr<GenericResponse<List<PermissionTypeDetailDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetPermissionTypesQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<List<PermissionTypeDetailDto>>>> Handle(
        GetPermissionTypesQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.PermissionTypes.AsQueryable();

        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(x => x.NameEn.Contains(request.SearchTerm) || x.NameAr.Contains(request.SearchTerm));

        var entities = await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.NameEn)
            .ToListAsync(cancellationToken);

        var dtos = entities.Select(e => new PermissionTypeDetailDto
        {
            Id = e.Id,
            NameAr = e.NameAr,
            NameEn = e.NameEn,
            Description = e.Description,
            RequiresManagerApproval = e.RequiresManagerApproval,
            RequireAttachment = e.RequireAttachment,
            IsActive = e.IsActive,
            SortOrder = e.SortOrder,
            CreatedDate = e.CreatedDate,
            ModifiedDate = e.ModifiedDate
        }).ToList();

        return GenericResponse<List<PermissionTypeDetailDto>>.SuccessResult(dtos);
    }
}
#endregion

#region Get PermissionType By Id
public record GetPermissionTypeByIdQuery(Guid Id) : IRequest<ErrorOr<GenericResponse<PermissionTypeDetailDto>>>;

public class GetPermissionTypeByIdQueryHandler : IRequestHandler<GetPermissionTypeByIdQuery, ErrorOr<GenericResponse<PermissionTypeDetailDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetPermissionTypeByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<PermissionTypeDetailDto>>> Handle(
        GetPermissionTypeByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await _context.PermissionTypes.FindAsync(new object[] { request.Id }, cancellationToken);
        if (entity == null)
            return Error.NotFound(description: "Permission type not found.");

        var dto = new PermissionTypeDetailDto
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

        return GenericResponse<PermissionTypeDetailDto>.SuccessResult(dto);
    }
}
#endregion
