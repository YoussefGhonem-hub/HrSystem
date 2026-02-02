using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.PersonalTypes;

#region Get All PersonalTypes
public record GetPersonalTypesQuery(
    bool? IsActive = null,
    string? SearchTerm = null
) : IRequest<ErrorOr<GenericResponse<List<PersonalTypeDetailDto>>>>;

public class GetPersonalTypesQueryHandler : IRequestHandler<GetPersonalTypesQuery, ErrorOr<GenericResponse<List<PersonalTypeDetailDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetPersonalTypesQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<List<PersonalTypeDetailDto>>>> Handle(
        GetPersonalTypesQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.PersonalTypes.AsQueryable();

        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(x => x.NameEn.Contains(request.SearchTerm) || x.NameAr.Contains(request.SearchTerm));

        var entities = await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.NameEn)
            .ToListAsync(cancellationToken);

        var dtos = entities.Select(e => new PersonalTypeDetailDto
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

        return GenericResponse<List<PersonalTypeDetailDto>>.SuccessResult(dtos);
    }
}
#endregion

#region Get PersonalType By Id
public record GetPersonalTypeByIdQuery(Guid Id) : IRequest<ErrorOr<GenericResponse<PersonalTypeDetailDto>>>;

public class GetPersonalTypeByIdQueryHandler : IRequestHandler<GetPersonalTypeByIdQuery, ErrorOr<GenericResponse<PersonalTypeDetailDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetPersonalTypeByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<PersonalTypeDetailDto>>> Handle(
        GetPersonalTypeByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await _context.PersonalTypes.FindAsync(new object[] { request.Id }, cancellationToken);
        if (entity == null)
            return Error.NotFound(description: "Personal type not found.");

        var dto = new PersonalTypeDetailDto
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

        return GenericResponse<PersonalTypeDetailDto>.SuccessResult(dto);
    }
}
#endregion
