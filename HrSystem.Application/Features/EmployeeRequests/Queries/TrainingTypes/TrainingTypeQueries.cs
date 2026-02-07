using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.TrainingTypes;

#region Get All TrainingTypes
public record GetTrainingTypesQuery(
    bool? IsActive = null,
    string? SearchTerm = null
) : IRequest<ErrorOr<GenericResponse<List<TrainingTypeDetailDto>>>>;

public class GetTrainingTypesQueryHandler : IRequestHandler<GetTrainingTypesQuery, ErrorOr<GenericResponse<List<TrainingTypeDetailDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetTrainingTypesQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<List<TrainingTypeDetailDto>>>> Handle(
        GetTrainingTypesQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.TrainingTypes.AsQueryable();

        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(x => x.NameEn.Contains(request.SearchTerm) || x.NameAr.Contains(request.SearchTerm));

        var entities = await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.NameEn)
            .ToListAsync(cancellationToken);

        var dtos = entities.Select(e => new TrainingTypeDetailDto
        {
            Id = e.Id,
            NameAr = e.NameAr,
            NameEn = e.NameEn,
            Description = e.Description,
            RequiresManagerApproval = e.RequiresManagerApproval,
            IsActive = e.IsActive,
            SortOrder = e.SortOrder,
            CreatedDate = e.CreatedDate,
            ModifiedDate = e.ModifiedDate
        }).ToList();

        return GenericResponse<List<TrainingTypeDetailDto>>.SuccessResult(dtos);
    }
}
#endregion

#region Get TrainingType By Id
public record GetTrainingTypeByIdQuery(Guid Id) : IRequest<ErrorOr<GenericResponse<TrainingTypeDetailDto>>>;

public class GetTrainingTypeByIdQueryHandler : IRequestHandler<GetTrainingTypeByIdQuery, ErrorOr<GenericResponse<TrainingTypeDetailDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetTrainingTypeByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<TrainingTypeDetailDto>>> Handle(
        GetTrainingTypeByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await _context.TrainingTypes.FindAsync(new object[] { request.Id }, cancellationToken);
        if (entity == null)
            return Error.NotFound(description: "Training type not found.");

        var dto = new TrainingTypeDetailDto
        {
            Id = entity.Id,
            NameAr = entity.NameAr,
            NameEn = entity.NameEn,
            Description = entity.Description,
            RequiresManagerApproval = entity.RequiresManagerApproval,
            IsActive = entity.IsActive,
            SortOrder = entity.SortOrder,
            CreatedDate = entity.CreatedDate,
            ModifiedDate = entity.ModifiedDate
        };

        return GenericResponse<TrainingTypeDetailDto>.SuccessResult(dto);
    }
}
#endregion
