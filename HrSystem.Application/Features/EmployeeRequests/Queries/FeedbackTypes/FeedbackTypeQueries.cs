using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.FeedbackTypes;

#region Get All FeedbackTypes
public record GetFeedbackTypesQuery(
    bool? IsActive = null,
    string? SearchTerm = null
) : IRequest<ErrorOr<GenericResponse<List<FeedbackTypeDetailDto>>>>;

public class GetFeedbackTypesQueryHandler : IRequestHandler<GetFeedbackTypesQuery, ErrorOr<GenericResponse<List<FeedbackTypeDetailDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetFeedbackTypesQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<List<FeedbackTypeDetailDto>>>> Handle(
        GetFeedbackTypesQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.FeedbackTypes.AsQueryable();

        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(x => x.NameEn.Contains(request.SearchTerm) || x.NameAr.Contains(request.SearchTerm));

        var entities = await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.NameEn)
            .ToListAsync(cancellationToken);

        var dtos = entities.Select(e => new FeedbackTypeDetailDto
        {
            Id = e.Id,
            NameAr = e.NameAr,
            NameEn = e.NameEn,
            Description = e.Description,
            IsAnonymousAllowed = e.IsAnonymousAllowed,
            RequiresManagerApproval = e.RequiresManagerApproval,
            RequireAttachment = e.RequireAttachment,
            IsActive = e.IsActive,
            SortOrder = e.SortOrder,
            CreatedDate = e.CreatedDate,
            ModifiedDate = e.ModifiedDate
        }).ToList();

        return GenericResponse<List<FeedbackTypeDetailDto>>.SuccessResult(dtos);
    }
}
#endregion

#region Get FeedbackType By Id
public record GetFeedbackTypeByIdQuery(Guid Id) : IRequest<ErrorOr<GenericResponse<FeedbackTypeDetailDto>>>;

public class GetFeedbackTypeByIdQueryHandler : IRequestHandler<GetFeedbackTypeByIdQuery, ErrorOr<GenericResponse<FeedbackTypeDetailDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetFeedbackTypeByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<FeedbackTypeDetailDto>>> Handle(
        GetFeedbackTypeByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await _context.FeedbackTypes.FindAsync(new object[] { request.Id }, cancellationToken);
        if (entity == null)
            return Error.NotFound(description: "Feedback type not found.");

        var dto = new FeedbackTypeDetailDto
        {
            Id = entity.Id,
            NameAr = entity.NameAr,
            NameEn = entity.NameEn,
            Description = entity.Description,
            IsAnonymousAllowed = entity.IsAnonymousAllowed,
            RequiresManagerApproval = entity.RequiresManagerApproval,
            RequireAttachment = entity.RequireAttachment,
            IsActive = entity.IsActive,
            SortOrder = entity.SortOrder,
            CreatedDate = entity.CreatedDate,
            ModifiedDate = entity.ModifiedDate
        };

        return GenericResponse<FeedbackTypeDetailDto>.SuccessResult(dto);
    }
}
#endregion
