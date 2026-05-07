using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.AttendanceCorrectionTypes;

#region Get All AttendanceCorrectionTypes
public record GetAttendanceCorrectionTypesQuery(
    bool? IsActive = null,
    string? SearchTerm = null
) : IRequest<ErrorOr<GenericResponse<List<AttendanceCorrectionTypeDetailDto>>>>;

public class GetAttendanceCorrectionTypesQueryHandler : IRequestHandler<GetAttendanceCorrectionTypesQuery, ErrorOr<GenericResponse<List<AttendanceCorrectionTypeDetailDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetAttendanceCorrectionTypesQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<List<AttendanceCorrectionTypeDetailDto>>>> Handle(
        GetAttendanceCorrectionTypesQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.AttendanceCorrectionTypes.AsQueryable();

        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(x => x.NameEn.Contains(request.SearchTerm) || x.NameAr.Contains(request.SearchTerm));

        var entities = await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.NameEn)
            .ToListAsync(cancellationToken);

        var dtos = entities.Select(e => new AttendanceCorrectionTypeDetailDto
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

        return GenericResponse<List<AttendanceCorrectionTypeDetailDto>>.SuccessResult(dtos);
    }
}
#endregion

#region Get AttendanceCorrectionType By Id
public record GetAttendanceCorrectionTypeByIdQuery(Guid Id) : IRequest<ErrorOr<GenericResponse<AttendanceCorrectionTypeDetailDto>>>;

public class GetAttendanceCorrectionTypeByIdQueryHandler : IRequestHandler<GetAttendanceCorrectionTypeByIdQuery, ErrorOr<GenericResponse<AttendanceCorrectionTypeDetailDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetAttendanceCorrectionTypeByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<AttendanceCorrectionTypeDetailDto>>> Handle(
        GetAttendanceCorrectionTypeByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await _context.AttendanceCorrectionTypes.FindAsync(new object[] { request.Id }, cancellationToken);
        if (entity == null)
            return Error.NotFound(description: "Attendance correction type not found.");

        var dto = new AttendanceCorrectionTypeDetailDto
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

        return GenericResponse<AttendanceCorrectionTypeDetailDto>.SuccessResult(dto);
    }
}
#endregion
