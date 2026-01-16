using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.Goals.Queries.GetGoalsList;

public class GetGoalsListQueryHandler : IRequestHandler<GetGoalsListQuery, ErrorOr<GenericResponse<PagedResult<GoalListDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetGoalsListQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<PagedResult<GoalListDto>>>> Handle(
        GetGoalsListQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Goals
            .Include(g => g.Employee)
            .AsQueryable();

        query = query.ApplyFilters(
            request.EmployeeId,
            request.Status,
            request.Priority,
            request.StartDateFrom,
            request.StartDateTo,
            request.TargetDateFrom,
            request.TargetDateTo);

        var totalCount = await query.CountAsync(cancellationToken);

        query = query.ApplySorting(request.SortBy, request.IsDescending);
        query = query.ApplyPaging(request.PageNumber, request.PageSize);

        var goals = await query.ToListAsync(cancellationToken);

        var dtos = goals.Select(g => new GoalListDto
        {
            Id = g.Id,
            EmployeeId = g.EmployeeId,
            EmployeeName = g.Employee?.FullNameEn ?? string.Empty,
            TitleAr = g.TitleAr,
            TitleEn = g.TitleEn,
            StartDate = g.StartDate,
            TargetDate = g.TargetDate,
            CompletionDate = g.CompletionDate,
            Status = g.Status,
            Progress = g.Progress,
            Priority = g.Priority
        }).ToList();

        var pagedResult = new PagedResult<GoalListDto>
        {
            Items = dtos,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };

        return new GenericResponse<PagedResult<GoalListDto>>
        {
            Success = true,
            Data = pagedResult
        };
    }
}
