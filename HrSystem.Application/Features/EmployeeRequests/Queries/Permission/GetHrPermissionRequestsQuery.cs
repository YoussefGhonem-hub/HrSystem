using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.Permission;

public record GetHrPermissionRequestsQuery(
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

public class GetHrPermissionRequestsQueryHandler : IRequestHandler<GetHrPermissionRequestsQuery, ErrorOr<GenericResponse<PagedResult<PermissionRequestListDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetHrPermissionRequestsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<PagedResult<PermissionRequestListDto>>>> Handle(GetHrPermissionRequestsQuery request, CancellationToken cancellationToken)
    {
        var roles = CurrentUser.Roles ?? Array.Empty<string>();
        var isHr = roles.Contains(RoleNames.HRManager) || roles.Contains(RoleNames.OrganizationAdmin) || roles.Contains(RoleNames.HRSpecialist);
        if (!isHr)
        {
            return Error.Forbidden("Permission.HROnly", "Only HR roles can access branch permission requests");
        }

        var branchId = CurrentUser.BranchId;
        if (!branchId.HasValue)
        {
            return Error.Conflict("Permission.NoBranch", "Current HR user has no branch selected");
        }

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
            .Where(r => r.Employee.BranchId == branchId)
            .AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(r => r.Status == request.Status.Value);

        if (request.PermissionTypeId.HasValue)
            query = query.Where(r => r.PermissionDetail != null && r.PermissionDetail.PermissionTypeId == request.PermissionTypeId.Value);

        if (request.StartDateFrom.HasValue)
            query = query.Where(r => r.PermissionDetail != null && r.PermissionDetail.PermissionDate >= request.StartDateFrom.Value);

        if (request.StartDateTo.HasValue)
            query = query.Where(r => r.PermissionDetail != null && r.PermissionDetail.PermissionDate <= request.StartDateTo.Value);

        if (request.EmployeeId.HasValue)
            query = query.Where(r => r.EmployeeId == request.EmployeeId.Value);

        // Sorting
        query = ApplySorting(query, request.SortBy, request.SortDescending);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
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

        var paged = new PagedResult<PermissionRequestListDto>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };

        return new GenericResponse<PagedResult<PermissionRequestListDto>>
        {
            Success = true,
            Message = "HR permission requests retrieved successfully",
            Data = paged
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
