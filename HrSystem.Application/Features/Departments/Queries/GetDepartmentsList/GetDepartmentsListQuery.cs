using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Departments.Queries.GetDepartmentsList;

public record GetDepartmentsListQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    Guid? BranchId = null,
    string? SortBy = null,
    bool SortDescending = false
) : IRequest<ErrorOr<GenericResponse<PagedResult<DepartmentListDto>>>>;

public class GetDepartmentsListQueryHandler : IRequestHandler<GetDepartmentsListQuery, ErrorOr<GenericResponse<PagedResult<DepartmentListDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetDepartmentsListQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<PagedResult<DepartmentListDto>>>> Handle(
        GetDepartmentsListQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Departments
            .Include(d => d.Manager)
            .Include(d => d.Branch)
            .Include(d => d.Employees)
            .AsQueryable();

        query = query.ApplyFilters(request.SearchTerm, request.BranchId);

        var totalCount = await query.CountAsync(cancellationToken);

        var departments = await query
            .ApplySorting(request.SortBy, request.SortDescending)
            .ApplyPaging(request.PageNumber, request.PageSize)
            .ToListAsync(cancellationToken);

        var departmentDtos = departments.Adapt<List<DepartmentListDto>>();

        var pagedResult = new PagedResult<DepartmentListDto>
        {
            Items = departmentDtos,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };

        return new GenericResponse<PagedResult<DepartmentListDto>>
        {
            Success = true,
            Data = pagedResult
        };
    }
}
