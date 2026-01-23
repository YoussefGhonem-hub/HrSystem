using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Leave.Queries.GetLeaveRequests;

/// <summary>
/// Query to get leave requests based on current user's role
/// - Employee: Gets all their own leave requests
/// - Department Manager: Gets pending requests from direct reports
/// - HR Manager: Gets manager-approved requests waiting for HR approval
/// </summary>
public record GetLeaveRequestsQuery(
    Guid? StatusId = null,
    Guid? LeaveTypeId = null,
    DateTime? StartDateFrom = null,
    DateTime? StartDateTo = null,
    Guid? EmployeeId = null,
    string? SortBy = null,
    bool SortDescending = false,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<ErrorOr<GenericResponse<PagedResult<LeaveRequestDto>>>>;

public class GetLeaveRequestsQueryHandler : IRequestHandler<GetLeaveRequestsQuery, ErrorOr<GenericResponse<PagedResult<LeaveRequestDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetLeaveRequestsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<PagedResult<LeaveRequestDto>>>> Handle(
        GetLeaveRequestsQuery request,
        CancellationToken cancellationToken)
    {
        // Get current user's employee record
        var currentEmployee = await _context.Employees
            .FirstOrDefaultAsync(e => e.UserId.ToString() == CurrentUser.UserId, cancellationToken);

        if (currentEmployee == null)
            return Error.Unauthorized("User.NotLinkedToEmployee", "Current user is not linked to an employee");

        // Determine user's role
        bool isHRManager = CurrentUser.Roles?.Contains(RoleNames.HRManager) == true ||
                          CurrentUser.Roles?.Contains(RoleNames.Admin) == true;

        bool isDepartmentManager = CurrentUser.Roles?.Contains(RoleNames.DepartmentManager) == true ||
                                   CurrentUser.Roles?.Contains(RoleNames.Manager) == true;

        bool isEmployee = !isHRManager && !isDepartmentManager;

        // Build query with includes
        var query = _context.LeaveRequests
            .Include(lr => lr.Employee)
                .ThenInclude(e => e.Department)
            .Include(lr => lr.Employee)
                .ThenInclude(e => e.JobTitle)
            .Include(lr => lr.Employee)
                .ThenInclude(e => e.Branch)
            .Include(lr => lr.LeavePolicy)
            .Include(lr => lr.LeaveStatus)
            .Include(lr => lr.LeaveType)
            .AsQueryable();

        // Apply role-based filters
        query = query.ApplyRoleBasedFilters(
            currentEmployee.Id,
            isHRManager,
            isDepartmentManager,
            isEmployee);

        // Apply additional filters if provided
        query = query.ApplyStatusFilter(request.StatusId);
        query = query.ApplyLeaveTypeFilter(request.LeaveTypeId);
        query = query.ApplyDateRangeFilter(request.StartDateFrom, request.StartDateTo);

        // Allow HR/Managers to filter by specific employee if provided
        if (!isEmployee && request.EmployeeId.HasValue)
        {
            query = query.ApplyEmployeeFilter(request.EmployeeId);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting and pagination
        query = query.ApplySorting(request.SortBy, request.SortDescending);

        var leaveRequests = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(lr => new LeaveRequestDto
            {
                Id = lr.Id,
                EmployeeId = lr.EmployeeId,
                EmployeeCode = lr.Employee.EmployeeCode,
                EmployeeName = lr.Employee.FullNameEn,
                EmployeeNameAr = lr.Employee.FullNameAr,
                DepartmentName = lr.Employee.Department.NameEn,
                JobTitle = lr.Employee.JobTitle.TitleEn,
                BranchName = lr.Employee.Branch != null ? lr.Employee.Branch.NameEn : null,
                LeaveTypeId = lr.LeaveTypeId,
                LeaveTypeName = lr.LeaveType.NameEn,
                LeaveTypeNameAr = lr.LeaveType.NameAr,
                StartDate = lr.StartDate,
                EndDate = lr.EndDate,
                TotalDays = lr.TotalDays,
                Reason = lr.Reason,
                StatusId = lr.LeaveStatusId,
                StatusName = lr.LeaveStatus.NameEn,
                StatusNameAr = lr.LeaveStatus.NameAr,
                ManagerApprovalDate = lr.ManagerApprovalDate,
                ManagerComments = lr.ManagerComments,
                HRApprovalDate = lr.HRApprovalDate,
                HRComments = lr.HRComments,
                DocumentUrl = lr.DocumentUrl,
                CreatedDate = lr.CreatedDate,
                RequiresHRApproval = lr.LeavePolicy.RequiresHRApproval,
                CurrentApprovalLevel = lr.LeaveStatusId == LeaveStatusIds.Pending ? "Manager" :
                                     lr.LeaveStatusId == LeaveStatusIds.ManagerApproved ? "HR" : "Completed"
            })
            .ToListAsync(cancellationToken);

        var pagedResult = new PagedResult<LeaveRequestDto>
        {
            Items = leaveRequests,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };

        return new GenericResponse<PagedResult<LeaveRequestDto>>
        {
            Success = true,
            Message = "Leave requests retrieved successfully",
            Data = pagedResult
        };
    }
}

