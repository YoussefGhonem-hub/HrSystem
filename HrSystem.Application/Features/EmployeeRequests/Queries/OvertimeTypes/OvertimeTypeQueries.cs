using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.OvertimeTypes;

#region Get All OvertimeTypes
public record GetOvertimeTypesQuery(
    bool? IsActive = null,
    string? SearchTerm = null
) : IRequest<ErrorOr<GenericResponse<List<OvertimeTypeDetailDto>>>>;

public class GetOvertimeTypesQueryHandler : IRequestHandler<GetOvertimeTypesQuery, ErrorOr<GenericResponse<List<OvertimeTypeDetailDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetOvertimeTypesQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<List<OvertimeTypeDetailDto>>>> Handle(
        GetOvertimeTypesQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.OvertimeTypes.AsQueryable();

        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(x => x.NameEn.Contains(request.SearchTerm) || x.NameAr.Contains(request.SearchTerm));

        var entities = await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.NameEn)
            .ToListAsync(cancellationToken);

        var dtos = entities.Select(e => new OvertimeTypeDetailDto
        {
            Id = e.Id,
            NameAr = e.NameAr,
            NameEn = e.NameEn,
            Description = e.Description,
            DefaultMultiplier = e.DefaultMultiplier,
            RequiresManagerApproval = e.RequiresManagerApproval,
            IsActive = e.IsActive,
            SortOrder = e.SortOrder,
            CreatedDate = e.CreatedDate,
            ModifiedDate = e.ModifiedDate
        }).ToList();

        return GenericResponse<List<OvertimeTypeDetailDto>>.SuccessResult(dtos);
    }
}
#endregion

#region Get OvertimeType By Id
public record GetOvertimeTypeByIdQuery(Guid Id) : IRequest<ErrorOr<GenericResponse<OvertimeTypeDetailDto>>>;

public class GetOvertimeTypeByIdQueryHandler : IRequestHandler<GetOvertimeTypeByIdQuery, ErrorOr<GenericResponse<OvertimeTypeDetailDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetOvertimeTypeByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<OvertimeTypeDetailDto>>> Handle(
        GetOvertimeTypeByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await _context.OvertimeTypes.FindAsync(new object[] { request.Id }, cancellationToken);
        if (entity == null)
            return Error.NotFound(description: "Overtime type not found.");

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

        return GenericResponse<OvertimeTypeDetailDto>.SuccessResult(dto);
    }
}
#endregion
