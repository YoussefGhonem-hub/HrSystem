using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Lifecycle.OnboardingTasks.Queries.GetOnboardingTasksList;

public class GetOnboardingTasksListQueryHandler : IRequestHandler<GetOnboardingTasksListQuery, ErrorOr<GenericResponse<PagedResult<OnboardingTaskListDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetOnboardingTasksListQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<PagedResult<OnboardingTaskListDto>>>> Handle(
        GetOnboardingTasksListQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.OnboardingTasks
            .Include(t => t.Employee)
            .AsQueryable();

        query = query.ApplyFilters(
            request.EmployeeId,
            request.IsCompleted,
            request.Category,
            request.DueDateFrom,
            request.DueDateTo);

        var totalCount = await query.CountAsync(cancellationToken);

        query = query.ApplySorting(request.SortBy, request.IsDescending);
        query = query.ApplyPaging(request.PageNumber, request.PageSize);

        var tasks = await query.ToListAsync(cancellationToken);

        var dtos = tasks.Select(t => new OnboardingTaskListDto
        {
            Id = t.Id,
            EmployeeId = t.EmployeeId,
            EmployeeName = t.Employee?.FullNameEn ?? string.Empty,
            TaskNameEn = t.TaskNameEn,
            Sequence = t.Sequence,
            DueDate = t.DueDate,
            IsCompleted = t.IsCompleted,
            CompletionDate = t.CompletionDate,
            Category = t.Category
        }).ToList();

        var pagedResult = new PagedResult<OnboardingTaskListDto>
        {
            Items = dtos,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };

        return new GenericResponse<PagedResult<OnboardingTaskListDto>>
        {
            Success = true,
            Data = pagedResult
        };
    }
}
