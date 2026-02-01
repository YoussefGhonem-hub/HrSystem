using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Application.Features.Leave.Queries.GetLeaveRequests;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Leave.Queries.GetMyLeaveRequests;

public record GetMyLeaveRequestsQuery(
    Guid? StatusId = null,
    Guid? LeaveTypeId = null,
    DateTime? StartDateFrom = null,
    DateTime? StartDateTo = null,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<ErrorOr<GenericResponse<MyLeaveRequestsResponse>>>;

public class GetMyLeaveRequestsQueryHandler : IRequestHandler<GetMyLeaveRequestsQuery, ErrorOr<GenericResponse<MyLeaveRequestsResponse>>>
{
    private readonly ApplicationDbContext _context;

    public GetMyLeaveRequestsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<MyLeaveRequestsResponse>>> Handle(
        GetMyLeaveRequestsQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(CurrentUser.UserId))
        {
            return Error.Unauthorized("User.NotAuthenticated", "Current user context is missing");
        }

        var currentEmployee = await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.UserId.ToString() == CurrentUser.UserId, cancellationToken);

        if (currentEmployee == null)
        {
            return Error.Unauthorized("User.NotLinkedToEmployee", "Current user is not linked to an employee");
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
            .Where(lr => lr.EmployeeId == currentEmployee.Id)
            .AsNoTracking();

        if (request.StatusId.HasValue)
        {
            query = query.Where(lr => lr.LeaveStatusId == request.StatusId.Value);
        }

        if (request.LeaveTypeId.HasValue)
        {
            query = query.Where(lr => lr.LeaveTypeId == request.LeaveTypeId.Value);
        }

        if (request.StartDateFrom.HasValue)
        {
            query = query.Where(lr => lr.StartDate >= request.StartDateFrom.Value);
        }

        if (request.StartDateTo.HasValue)
        {
            query = query.Where(lr => lr.StartDate <= request.StartDateTo.Value);
        }

        var filteredQuery = query;

        var statusStatistics = await filteredQuery
            .GroupBy(lr => new { lr.LeaveStatusId, lr.LeaveStatus.NameEn, lr.LeaveStatus.NameAr })
            .Select(g => new LeaveStatusStatisticDto
            {
                StatusId = g.Key.LeaveStatusId,
                StatusName = g.Key.NameEn,
                StatusNameAr = g.Key.NameAr,
                Total = g.Count()
            })
            .ToListAsync(cancellationToken);

        var totalCount = await filteredQuery.CountAsync(cancellationToken);

        // Latest created requests first for a quick timeline view
        var sortedQuery = filteredQuery.OrderByDescending(lr => lr.CreatedDate);

        var leaveRequests = await sortedQuery
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

        return new GenericResponse<MyLeaveRequestsResponse>
        {
            Success = true,
            Message = "My leave requests retrieved successfully",
            Data = new MyLeaveRequestsResponse
            {
                Requests = pagedResult,
                Statistics = statusStatistics
            }
        };
    }
}
