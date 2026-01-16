using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Application.Features.Employees.DTOs;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Employees.Queries.GetEmployeesList;

public class GetEmployeesListQueryHandler : IRequestHandler<GetEmployeesListQuery, ErrorOr<GenericResponse<PagedResult<EmployeeListDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetEmployeesListQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<PagedResult<EmployeeListDto>>>> Handle(
        GetEmployeesListQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Employees
            .Include(e => e.Department)
            .Include(e => e.JobTitle)
            .Include(e => e.Branch)
            .AsQueryable();

        // Apply filters
        query = query.ApplyFilters(
            request.SearchTerm,
            request.Status,
            request.DepartmentId,
            request.BranchId,
            request.JobTitleId,
            request.ManagerId);

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting and pagination
        var employees = await query
            .ApplySorting(request.SortBy, request.SortDescending)
            .ApplyPaging(request.PageNumber, request.PageSize)
            .ToListAsync(cancellationToken);

        // Map to DTOs using Mapster
        var employeeDtos = employees.Adapt<List<EmployeeListDto>>();

        var pagedResult = new PagedResult<EmployeeListDto>
        {
            Items = employeeDtos,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };

        return new GenericResponse<PagedResult<EmployeeListDto>>
        {
            Success = true,
            Data = pagedResult
        };
    }
}
