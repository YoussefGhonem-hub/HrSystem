using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.MiscellaneousTypes;

#region Get All MiscellaneousTypes
public record GetMiscellaneousTypesQuery(
    bool? IsActive = null,
    string? SearchTerm = null
) : IRequest<ErrorOr<GenericResponse<List<MiscellaneousTypeDetailDto>>>>;

public class GetMiscellaneousTypesQueryHandler : IRequestHandler<GetMiscellaneousTypesQuery, ErrorOr<GenericResponse<List<MiscellaneousTypeDetailDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetMiscellaneousTypesQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<List<MiscellaneousTypeDetailDto>>>> Handle(
        GetMiscellaneousTypesQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.MiscellaneousTypes.AsQueryable();

        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(x => x.NameEn.Contains(request.SearchTerm) || x.NameAr.Contains(request.SearchTerm));

        var entities = await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.NameEn)
            .ToListAsync(cancellationToken);

        var dtos = entities.Select(e => new MiscellaneousTypeDetailDto
        {
            Id = e.Id,
            NameAr = e.NameAr,
            NameEn = e.NameEn,
            Description = e.Description,
            RequiresAttachment = e.RequiresAttachment,
            RequiresManagerApproval = e.RequiresManagerApproval,
            IsActive = e.IsActive,
            SortOrder = e.SortOrder,
            CreatedDate = e.CreatedDate,
            ModifiedDate = e.ModifiedDate
        }).ToList();

        return GenericResponse<List<MiscellaneousTypeDetailDto>>.SuccessResult(dtos);
    }
}
#endregion

#region Get MiscellaneousType By Id
public record GetMiscellaneousTypeByIdQuery(Guid Id) : IRequest<ErrorOr<GenericResponse<MiscellaneousTypeDetailDto>>>;

public class GetMiscellaneousTypeByIdQueryHandler : IRequestHandler<GetMiscellaneousTypeByIdQuery, ErrorOr<GenericResponse<MiscellaneousTypeDetailDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetMiscellaneousTypeByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<MiscellaneousTypeDetailDto>>> Handle(
        GetMiscellaneousTypeByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await _context.MiscellaneousTypes.FindAsync(new object[] { request.Id }, cancellationToken);
        if (entity == null)
            return Error.NotFound(description: "Miscellaneous type not found.");

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

        return GenericResponse<MiscellaneousTypeDetailDto>.SuccessResult(dto);
    }
}
#endregion
