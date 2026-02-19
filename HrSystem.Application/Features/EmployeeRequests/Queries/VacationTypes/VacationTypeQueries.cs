using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.VacationTypes;

#region Get All VacationTypes
public record GetVacationTypesQuery(
    bool? IsActive = null,
    string? SearchTerm = null
) : IRequest<ErrorOr<GenericResponse<List<VacationTypeDetailDto>>>>;

public class GetVacationTypesQueryHandler : IRequestHandler<GetVacationTypesQuery, ErrorOr<GenericResponse<List<VacationTypeDetailDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetVacationTypesQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<List<VacationTypeDetailDto>>>> Handle(
        GetVacationTypesQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.VacationTypes.AsQueryable();

        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(x => x.NameEn.Contains(request.SearchTerm) || x.NameAr.Contains(request.SearchTerm));

        var entities = await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.NameEn)
            .ToListAsync(cancellationToken);

        var dtos = entities.Select(e => new VacationTypeDetailDto
        {
            Id = e.Id,
            NameAr = e.NameAr,
            NameEn = e.NameEn,
            Description = e.Description,
            IsPaid = e.IsPaid,
            RequiresManagerApproval = e.RequiresManagerApproval,
            RequireAttachment = e.RequireAttachment,
            IsActive = e.IsActive,
            SortOrder = e.SortOrder,
            MaxDaysPerYear = e.MaxDaysPerYear,
            CreatedDate = e.CreatedDate,
            ModifiedDate = e.ModifiedDate
        }).ToList();

        return GenericResponse<List<VacationTypeDetailDto>>.SuccessResult(dtos);
    }
}
#endregion

#region Get VacationType By Id
public record GetVacationTypeByIdQuery(Guid Id) : IRequest<ErrorOr<GenericResponse<VacationTypeDetailDto>>>;

public class GetVacationTypeByIdQueryHandler : IRequestHandler<GetVacationTypeByIdQuery, ErrorOr<GenericResponse<VacationTypeDetailDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetVacationTypeByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<VacationTypeDetailDto>>> Handle(
        GetVacationTypeByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await _context.VacationTypes.FindAsync(new object[] { request.Id }, cancellationToken);
        if (entity == null)
            return Error.NotFound(description: "Vacation type not found.");

        var dto = new VacationTypeDetailDto
        {
            Id = entity.Id,
            NameAr = entity.NameAr,
            NameEn = entity.NameEn,
            Description = entity.Description,
            IsPaid = entity.IsPaid,
            RequiresManagerApproval = entity.RequiresManagerApproval,
            RequireAttachment = entity.RequireAttachment,
            IsActive = entity.IsActive,
            SortOrder = entity.SortOrder,
            MaxDaysPerYear = entity.MaxDaysPerYear,
            CreatedDate = entity.CreatedDate,
            ModifiedDate = entity.ModifiedDate
        };

        return GenericResponse<VacationTypeDetailDto>.SuccessResult(dto);
    }
}
#endregion
