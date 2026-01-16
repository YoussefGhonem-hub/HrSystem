using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Application.Features.Performance.GoalPriorities.Common;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.GoalPriorities.Queries.GetGoalPrioritiesList;

public record GetGoalPrioritiesListQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? NameAr = null,
    string? NameEn = null,
    bool? IsActive = null,
    string? SortBy = null,
    bool SortDescending = false
) : IRequest<ErrorOr<GenericResponse<PagedResult<GoalPriorityListDto>>>>;

public class GetGoalPrioritiesListQueryHandler : IRequestHandler<GetGoalPrioritiesListQuery, ErrorOr<GenericResponse<PagedResult<GoalPriorityListDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetGoalPrioritiesListQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<PagedResult<GoalPriorityListDto>>>> Handle(
        GetGoalPrioritiesListQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.GoalPriorities
            .Include(gp => gp.Goals)
            .AsQueryable();

        query = query.ApplyFilters(request.NameAr, request.NameEn, request.IsActive);

        var totalCount = await query.CountAsync(cancellationToken);

        var goalPriorities = await query
            .ApplySorting(request.SortBy, request.SortDescending)
            .ApplyPaging(request.PageNumber, request.PageSize)
            .ToListAsync(cancellationToken);

        var goalPriorityDtos = goalPriorities.Adapt<List<GoalPriorityListDto>>();

        var pagedResult = new PagedResult<GoalPriorityListDto>
        {
            Items = goalPriorityDtos,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };

        return new GenericResponse<PagedResult<GoalPriorityListDto>>
        {
            Success = true,
            Data = pagedResult
        };
    }
}
