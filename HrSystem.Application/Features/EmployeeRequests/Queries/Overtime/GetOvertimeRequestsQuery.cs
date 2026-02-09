using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.Overtime;

public record GetOvertimeRequestsQuery(
    EmployeeRequestStatus? Status = null,
    Guid? OvertimeTypeId = null,
    DateTime? StartDateFrom = null,
    DateTime? StartDateTo = null,
    Guid? EmployeeId = null,
    string? SortBy = null,
    bool SortDescending = false,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<ErrorOr<GenericResponse<PagedResult<OvertimeRequestListDto>>>>;

public class GetOvertimeRequestsQueryHandler
    : IRequestHandler<GetOvertimeRequestsQuery, ErrorOr<GenericResponse<PagedResult<OvertimeRequestListDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetOvertimeRequestsQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<PagedResult<OvertimeRequestListDto>>>> Handle(
        GetOvertimeRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var currentEmployee = await _context.Employees
            .FirstOrDefaultAsync(e => e.UserId.ToString() == CurrentUser.UserId, cancellationToken);

        if (currentEmployee == null)
            return Error.Unauthorized("User.NotLinkedToEmployee", "Current user is not linked to an employee");

        bool isHRManager = CurrentUser.Roles?.Contains(RoleNames.HRManager) == true ||
                          CurrentUser.Roles?.Contains(RoleNames.OrganizationAdmin) == true ||
                          CurrentUser.Roles?.Contains(RoleNames.HRSpecialist) == true;

        bool isDepartmentManager = CurrentUser.Roles?.Contains(RoleNames.DepartmentManager) == true;
        bool isEmployee = !isHRManager && !isDepartmentManager;

        var query = _context.EmployeeRequests
            .Include(r => r.RequestTypeRef)
            .Include(r => r.Employee).ThenInclude(e => e.Department)
            .Include(r => r.Employee).ThenInclude(e => e.JobTitle)
            .Include(r => r.Employee).ThenInclude(e => e.Branch)
            .Include(r => r.OvertimeDetail).ThenInclude(o => o!.OvertimeType)
            .Where(r => r.RequestTypeRef != null && r.RequestTypeRef.Code == "OverTime")
            .AsQueryable();

        if (isEmployee)
        {
            query = query.Where(r => r.EmployeeId == currentEmployee.Id);
        }
        else if (isDepartmentManager && !isHRManager)
        {
            query = query.Where(r =>
                r.Status == EmployeeRequestStatus.Pending &&
                r.Employee.DirectManagerId == currentEmployee.Id);
        }
        else if (isHRManager)
        {
            query = query.Where(r => r.Status == EmployeeRequestStatus.ManagerApproved);
        }

        if (request.Status.HasValue)
            query = query.Where(r => r.Status == request.Status.Value);

        if (request.OvertimeTypeId.HasValue)
            query = query.Where(r => r.OvertimeDetail != null && r.OvertimeDetail.OvertimeTypeId == request.OvertimeTypeId.Value);

        if (request.StartDateFrom.HasValue)
            query = query.Where(r => r.OvertimeDetail != null && r.OvertimeDetail.OvertimeDate >= request.StartDateFrom.Value);

        if (request.StartDateTo.HasValue)
            query = query.Where(r => r.OvertimeDetail != null && r.OvertimeDetail.OvertimeDate <= request.StartDateTo.Value);

        if (!isEmployee && request.EmployeeId.HasValue)
            query = query.Where(r => r.EmployeeId == request.EmployeeId.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        query = ApplySorting(query, request.SortBy, request.SortDescending);

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(r => new OvertimeRequestListDto
            {
                Id = r.Id,
                EmployeeId = r.EmployeeId,
                EmployeeCode = r.Employee.EmployeeCode,
                EmployeeName = r.Employee.FullNameEn,
                EmployeeNameAr = r.Employee.FullNameAr,
                DepartmentName = r.Employee.Department.NameEn,
                JobTitle = r.Employee.JobTitle.TitleEn,
                BranchName = r.Employee.Branch != null ? r.Employee.Branch.NameEn : null,
                OvertimeTypeId = r.OvertimeDetail!.OvertimeTypeId,
                OvertimeTypeName = r.OvertimeDetail.OvertimeType != null ? r.OvertimeDetail.OvertimeType.NameEn : null,
                OvertimeTypeNameAr = r.OvertimeDetail.OvertimeType != null ? r.OvertimeDetail.OvertimeType.NameAr : null,
                OvertimeDate = r.OvertimeDetail.OvertimeDate,
                PlannedHours = r.OvertimeDetail.PlannedHours,
                ActualHours = r.OvertimeDetail.ActualHours,
                Multiplier = r.OvertimeDetail.Multiplier,
                ProjectCode = r.OvertimeDetail.ProjectCode,
                TaskDescription = r.OvertimeDetail.TaskDescription,
                Title = r.Title,
                Description = r.Description,
                Status = r.Status,
                StatusName = r.Status.ToString(),
                RequestedDate = r.RequestedDate,
                ApprovedDate = r.ApprovedDate,
                RejectionReason = r.RejectionReason,
                CurrentApprovalLevel = r.Status == EmployeeRequestStatus.Pending ? 1 :
                                      r.Status == EmployeeRequestStatus.ManagerApproved ? 2 : 0
            })
            .ToListAsync(cancellationToken);

        var pagedResult = PagedResult<OvertimeRequestListDto>.Create(items, totalCount, request.PageNumber, request.PageSize);
        return GenericResponse<PagedResult<OvertimeRequestListDto>>.SuccessResult(pagedResult, "Overtime requests retrieved successfully");
    }

    private static IQueryable<Domain.Entities.Requests.EmployeeRequest> ApplySorting(
        IQueryable<Domain.Entities.Requests.EmployeeRequest> query, string? sortBy, bool descending)
    {
        return sortBy?.ToLower() switch
        {
            "overtimedate" => descending
                ? query.OrderByDescending(r => r.OvertimeDetail!.OvertimeDate)
                : query.OrderBy(r => r.OvertimeDetail!.OvertimeDate),
            "status" => descending
                ? query.OrderByDescending(r => r.Status)
                : query.OrderBy(r => r.Status),
            "employee" => descending
                ? query.OrderByDescending(r => r.Employee.FullNameEn)
                : query.OrderBy(r => r.Employee.FullNameEn),
            _ => query.OrderByDescending(r => r.RequestedDate)
        };
    }
}
