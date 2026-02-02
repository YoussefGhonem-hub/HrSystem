using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.Permission;

/// <summary>
/// Query to get permission requests based on current user's role
/// - Employee: Gets all their own permission requests
/// - Department Manager: Gets pending requests from direct reports
/// - HR Manager: Gets manager-approved requests waiting for HR approval
/// </summary>
public record GetPermissionRequestsQuery(
    EmployeeRequestStatus? Status = null,
    Guid? PermissionTypeId = null,
    DateTime? StartDateFrom = null,
    DateTime? StartDateTo = null,
    Guid? EmployeeId = null,
    string? SortBy = null,
    bool SortDescending = false,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<ErrorOr<GenericResponse<PagedResult<PermissionRequestListDto>>>>;

public class GetPermissionRequestsQueryHandler : IRequestHandler<GetPermissionRequestsQuery, ErrorOr<GenericResponse<PagedResult<PermissionRequestListDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetPermissionRequestsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<PagedResult<PermissionRequestListDto>>>> Handle(
        GetPermissionRequestsQuery request,
        CancellationToken cancellationToken)
    {
        // Get current user's employee record
        var currentEmployee = await _context.Employees
            .FirstOrDefaultAsync(e => e.UserId.ToString() == CurrentUser.UserId, cancellationToken);

        if (currentEmployee == null)
            return Error.Unauthorized("User.NotLinkedToEmployee", "Current user is not linked to an employee");

        // Determine user's role
        bool isHRManager = CurrentUser.Roles?.Contains(RoleNames.HRManager) == true ||
                          CurrentUser.Roles?.Contains(RoleNames.OrganizationAdmin) == true ||
                          CurrentUser.Roles?.Contains(RoleNames.HRSpecialist) == true;

        bool isDepartmentManager = CurrentUser.Roles?.Contains(RoleNames.DepartmentManager) == true;

        bool isEmployee = !isHRManager && !isDepartmentManager;

        // Build query with includes
        var query = _context.EmployeeRequests
            .Include(r => r.RequestTypeRef)
            .Include(r => r.Employee)
                .ThenInclude(e => e.Department)
            .Include(r => r.Employee)
                .ThenInclude(e => e.JobTitle)
            .Include(r => r.Employee)
                .ThenInclude(e => e.Branch)
            .Include(r => r.PermissionDetail)
                .ThenInclude(p => p!.PermissionType)
            .Where(r => r.RequestTypeRef != null && r.RequestTypeRef.Code == "Permission")
            .AsQueryable();

        // Apply role-based filters
        if (isEmployee)
        {
            // Employee: Get all their own permission requests
            query = query.Where(r => r.EmployeeId == currentEmployee.Id);
        }
        else if (isDepartmentManager && !isHRManager)
        {
            // Department Manager: Get requests pending approval from direct reports
            query = query.Where(r =>
                r.Status == EmployeeRequestStatus.Pending &&
                r.Employee.DirectManagerId == currentEmployee.Id);
        }
        else if (isHRManager)
        {
            // HR Manager: Get requests that are manager approved and waiting for HR approval
            query = query.Where(r => r.Status == EmployeeRequestStatus.ManagerApproved);
        }

        // Apply additional filters if provided
        if (request.Status.HasValue)
        {
            query = query.Where(r => r.Status == request.Status.Value);
        }

        if (request.PermissionTypeId.HasValue)
        {
            query = query.Where(r => r.PermissionDetail != null && r.PermissionDetail.PermissionTypeId == request.PermissionTypeId.Value);
        }

        if (request.StartDateFrom.HasValue)
        {
            query = query.Where(r => r.PermissionDetail != null && r.PermissionDetail.PermissionDate >= request.StartDateFrom.Value);
        }

        if (request.StartDateTo.HasValue)
        {
            query = query.Where(r => r.PermissionDetail != null && r.PermissionDetail.PermissionDate <= request.StartDateTo.Value);
        }

        // Allow HR/Managers to filter by specific employee if provided
        if (!isEmployee && request.EmployeeId.HasValue)
        {
            query = query.Where(r => r.EmployeeId == request.EmployeeId.Value);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = ApplySorting(query, request.SortBy, request.SortDescending);

        var permissionRequests = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(r => new PermissionRequestListDto
            {
                Id = r.Id,
                EmployeeId = r.EmployeeId,
                EmployeeCode = r.Employee.EmployeeCode,
                EmployeeName = r.Employee.FullNameEn,
                EmployeeNameAr = r.Employee.FullNameAr,
                DepartmentName = r.Employee.Department.NameEn,
                JobTitle = r.Employee.JobTitle.TitleEn,
                BranchName = r.Employee.Branch != null ? r.Employee.Branch.NameEn : null,
                PermissionTypeId = r.PermissionDetail!.PermissionTypeId,
                PermissionTypeName = r.PermissionDetail.PermissionType != null ? r.PermissionDetail.PermissionType.NameEn : null,
                PermissionTypeNameAr = r.PermissionDetail.PermissionType != null ? r.PermissionDetail.PermissionType.NameAr : null,
                PermissionDate = r.PermissionDetail.PermissionDate,
                FromTime = r.PermissionDetail.FromTime,
                ToTime = r.PermissionDetail.ToTime,
                TotalHours = r.PermissionDetail.TotalHours,
                Reason = r.PermissionDetail.Reason,
                Title = r.Title,
                Description = r.Description,
                Status = r.Status,
                StatusName = r.Status.ToString(),
                ManagerApprovalDate = r.PermissionDetail.ManagerApprovalDate,
                ManagerComments = r.PermissionDetail.ManagerComments,
                AttachmentUrl = r.AttachmentUrl,
                CreatedDate = r.CreatedDate,
                CurrentApprovalLevel = r.Status == EmployeeRequestStatus.Pending ? "Manager" :
                                       r.Status == EmployeeRequestStatus.ManagerApproved ? "HR" : "Completed"
            })
            .ToListAsync(cancellationToken);

        var pagedResult = new PagedResult<PermissionRequestListDto>
        {
            Items = permissionRequests,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };

        return new GenericResponse<PagedResult<PermissionRequestListDto>>
        {
            Success = true,
            Message = "Permission requests retrieved successfully",
            Data = pagedResult
        };
    }

    private static IQueryable<Domain.Entities.Requests.EmployeeRequest> ApplySorting(
        IQueryable<Domain.Entities.Requests.EmployeeRequest> query,
        string? sortBy,
        bool sortDescending)
    {
        if (string.IsNullOrEmpty(sortBy))
        {
            return sortDescending
                ? query.OrderByDescending(r => r.PermissionDetail!.PermissionDate)
                : query.OrderBy(r => r.PermissionDetail!.PermissionDate);
        }

        return sortBy.ToLower() switch
        {
            "permissiondate" => sortDescending ? query.OrderByDescending(r => r.PermissionDetail!.PermissionDate) : query.OrderBy(r => r.PermissionDetail!.PermissionDate),
            "totalhours" => sortDescending ? query.OrderByDescending(r => r.PermissionDetail!.TotalHours) : query.OrderBy(r => r.PermissionDetail!.TotalHours),
            "status" => sortDescending ? query.OrderByDescending(r => r.Status) : query.OrderBy(r => r.Status),
            "createddate" => sortDescending ? query.OrderByDescending(r => r.CreatedDate) : query.OrderBy(r => r.CreatedDate),
            "employeename" => sortDescending ? query.OrderByDescending(r => r.Employee.FullNameEn) : query.OrderBy(r => r.Employee.FullNameEn),
            _ => sortDescending ? query.OrderByDescending(r => r.PermissionDetail!.PermissionDate) : query.OrderBy(r => r.PermissionDetail!.PermissionDate)
        };
    }
}
