using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Application.Features.Performance.GoalStatuses.Common;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.GoalStatuses.Queries.GetGoalStatusesList;

public record GetGoalStatusesListQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? NameAr = null,
    string? NameEn = null,
    bool? IsActive = null,
    string? SortBy = null,
    bool SortDescending = false
) : IRequest<ErrorOr<GenericResponse<PagedResult<GoalStatusListDto>>>>;

public class GetGoalStatusesListQueryHandler : IRequestHandler<GetGoalStatusesListQuery, ErrorOr<GenericResponse<PagedResult<GoalStatusListDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetGoalStatusesListQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<PagedResult<GoalStatusListDto>>>> Handle(
        GetGoalStatusesListQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.GoalStatuses
            .Include(gs => gs.Goals)
            .AsQueryable();

        query = query.ApplyFilters(request.NameAr, request.NameEn, request.IsActive);

        var totalCount = await query.CountAsync(cancellationToken);

        var goalStatuses = await query
            .ApplySorting(request.SortBy, request.SortDescending)
            .ApplyPaging(request.PageNumber, request.PageSize)
            .ToListAsync(cancellationToken);

        var goalStatusDtos = goalStatuses.Adapt<List<GoalStatusListDto>>();

        var pagedResult = new PagedResult<GoalStatusListDto>
        {
            Items = goalStatusDtos,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };

        return new GenericResponse<PagedResult<GoalStatusListDto>>
        {
            Success = true,
            Data = pagedResult
        };
    }
}
