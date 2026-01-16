using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.GoalMilestones.Queries.GetGoalMilestonesList;

public class GetGoalMilestonesListQueryHandler : IRequestHandler<GetGoalMilestonesListQuery, ErrorOr<GenericResponse<PagedResult<GoalMilestoneListDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetGoalMilestonesListQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<PagedResult<GoalMilestoneListDto>>>> Handle(
        GetGoalMilestonesListQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.GoalMilestones
            .Include(gm => gm.Goal)
                .ThenInclude(g => g.Employee)
            .AsQueryable();

        // Apply filters
        query = query.ApplyFilters(
            request.GoalId,
            request.IsCompleted,
            request.DueDateFrom,
            request.DueDateTo);

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting and pagination
        var goalMilestones = await query
            .ApplySorting(request.SortBy, request.SortDescending)
            .ApplyPaging(request.PageNumber, request.PageSize)
            .ToListAsync(cancellationToken);

        // Map to DTOs
        var goalMilestoneDtos = goalMilestones.Select(gm => new GoalMilestoneListDto
        {
            Id = gm.Id,
            GoalId = gm.GoalId,
            GoalTitleEn = gm.Goal.TitleEn,
            GoalTitleAr = gm.Goal.TitleAr,
            EmployeeName = gm.Goal.Employee.FullNameEn,
            TitleAr = gm.TitleAr,
            TitleEn = gm.TitleEn,
            DueDate = gm.DueDate,
            IsCompleted = gm.IsCompleted,
            CompletionDate = gm.CompletionDate,
            CreatedDate = gm.CreatedDate.DateTime
        }).ToList();

        var pagedResult = new PagedResult<GoalMilestoneListDto>
        {
            Items = goalMilestoneDtos,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };

        return new GenericResponse<PagedResult<GoalMilestoneListDto>>
        {
            Success = true,
            Data = pagedResult
        };
    }
}
