using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Application.Features.Leave.Queries.GetLeaveRequests;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Leave.Queries.Hr.GetHrLeaveRequests;

public record GetHrLeaveRequestsQuery(
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

public class GetHrLeaveRequestsQueryHandler : IRequestHandler<GetHrLeaveRequestsQuery, ErrorOr<GenericResponse<PagedResult<LeaveRequestDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetHrLeaveRequestsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<PagedResult<LeaveRequestDto>>>> Handle(GetHrLeaveRequestsQuery request, CancellationToken cancellationToken)
    {
        var roles = CurrentUser.Roles ?? Array.Empty<string>();
        var isHr = roles.Contains(RoleNames.HRManager) || roles.Contains(RoleNames.OrganizationAdmin) || roles.Contains(RoleNames.HRSpecialist);
        if (!isHr)
        {
            return Error.Forbidden("Leave.HROnly", "Only HR roles can access branch leave requests");
        }

        var branchId = CurrentUser.BranchId;
        if (!branchId.HasValue)
        {
            return Error.Conflict("Leave.NoBranch", "Current HR user has no branch selected");
        }

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
            .Where(lr => lr.Employee.BranchId == branchId)
            .AsQueryable();

        if (request.StatusId.HasValue)
            query = query.Where(lr => lr.LeaveStatusId == request.StatusId.Value);
        if (request.LeaveTypeId.HasValue)
            query = query.Where(lr => lr.LeaveTypeId == request.LeaveTypeId.Value);
        if (request.StartDateFrom.HasValue)
            query = query.Where(lr => lr.StartDate >= request.StartDateFrom.Value);
        if (request.StartDateTo.HasValue)
            query = query.Where(lr => lr.StartDate <= request.StartDateTo.Value);
        if (request.EmployeeId.HasValue)
            query = query.Where(lr => lr.EmployeeId == request.EmployeeId.Value);

        // Sorting
        query = LeaveRequestFilterExtensions.ApplySorting(query, request.SortBy, request.SortDescending);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
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

        var paged = new PagedResult<LeaveRequestDto>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };

        return new GenericResponse<PagedResult<LeaveRequestDto>>
        {
            Success = true,
            Message = "HR leave requests retrieved successfully",
            Data = paged
        };
    }
}
