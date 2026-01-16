using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Lifecycle.OffboardingTasks.Queries.GetOffboardingTasksList;

public class GetOffboardingTasksListQueryHandler : IRequestHandler<GetOffboardingTasksListQuery, ErrorOr<GenericResponse<PagedResult<OffboardingTaskListDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetOffboardingTasksListQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<PagedResult<OffboardingTaskListDto>>>> Handle(
        GetOffboardingTasksListQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.OffboardingTasks
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

        var dtos = tasks.Select(t => new OffboardingTaskListDto
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

        var pagedResult = new PagedResult<OffboardingTaskListDto>
        {
            Items = dtos,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };

        return new GenericResponse<PagedResult<OffboardingTaskListDto>>
        {
            Success = true,
            Data = pagedResult
        };
    }
}
