using ErrorOr;
using HrSystem.Application.Common.Extensions;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Employees.Queries.GetEmployeesList;

public record GetEmployeesListQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    Guid? StatusId = null,
    Guid? DepartmentId = null,
    Guid? BranchId = null,
    Guid? JobTitleId = null,
    Guid? ManagerId = null,
    string? SortBy = null,
    bool SortDescending = false
) : IRequest<ErrorOr<GenericResponse<PagedResult<EmployeeListDto>>>>;

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
            .Include(e => e.Status)
            .ApplyBranchScope()
            .AsQueryable();

        // Apply filters
        query = query.ApplyFilters(
            request.SearchTerm,
            request.StatusId,
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

        // Load roles for the returned employees
        var userIds = employees
            .Where(e => e.UserId.HasValue)
            .Select(e => e.UserId!.Value)
            .Distinct()
            .ToList();

        if (userIds.Count > 0)
        {
            // Use Identity tables (AspNetUserRoles + AspNetRoles) — they have no global
            // tenant query filter, unlike UserBranchRoles which inherits BaseEntity.
            var userRoles = await (
                from ur in _context.UserRoles
                join r in _context.Roles on ur.RoleId equals r.Id
                where userIds.Contains(ur.UserId)
                select new { ur.UserId, RoleName = r.Name }
            ).ToListAsync(cancellationToken);

            var roleByUserId = userRoles
                .GroupBy(r => r.UserId)
                .ToDictionary(g => g.Key, g => g.First().RoleName);

            for (int i = 0; i < employees.Count; i++)
            {
                if (employees[i].UserId.HasValue &&
                    roleByUserId.TryGetValue(employees[i].UserId.Value, out var roleName))
                {
                    employeeDtos[i] = employeeDtos[i] with { RoleNameEn = roleName };
                }
            }
        }

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
