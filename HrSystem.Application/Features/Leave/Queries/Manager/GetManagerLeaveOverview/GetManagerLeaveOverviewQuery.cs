using ErrorOr;
using HrSystem.Application.Features.Leave.Queries.GetLeaveRequests;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Leave.Queries.Manager.GetManagerLeaveOverview;

public record GetManagerLeaveOverviewQuery(
    int MyPageNumber = 1,
    int MyPageSize = 10,
    int PendingPageNumber = 1,
    int PendingPageSize = 10
) : IRequest<ErrorOr<GenericResponse<ManagerLeaveOverviewDto>>>;

public class GetManagerLeaveOverviewQueryHandler : IRequestHandler<GetManagerLeaveOverviewQuery, ErrorOr<GenericResponse<ManagerLeaveOverviewDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetManagerLeaveOverviewQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<ManagerLeaveOverviewDto>>> Handle(GetManagerLeaveOverviewQuery request, CancellationToken cancellationToken)
    {
        var roles = CurrentUser.Roles ?? Array.Empty<string>();
        var isManager = roles.Contains(RoleNames.DepartmentManager) || roles.Contains(RoleNames.OrganizationAdmin);
        if (!isManager)
        {
            return Error.Forbidden("Leave.ManagerOnly", "Only department managers can access this overview");
        }

        var currentEmployee = await _context.Employees
            .FirstOrDefaultAsync(e => e.UserId.ToString() == CurrentUser.UserId, cancellationToken);
        if (currentEmployee == null)
            return Error.Unauthorized("User.NotLinkedToEmployee", "Current user is not linked to an employee");

        // My own requests
        var myQuery = _context.LeaveRequests
            .Include(lr => lr.Employee).ThenInclude(e => e.Department)
            .Include(lr => lr.Employee).ThenInclude(e => e.JobTitle)
            .Include(lr => lr.Employee).ThenInclude(e => e.Branch)
            .Include(lr => lr.LeavePolicy)
            .Include(lr => lr.LeaveStatus)
            .Include(lr => lr.LeaveType)
            .Where(lr => lr.EmployeeId == currentEmployee.Id)
            .OrderByDescending(lr => lr.CreatedDate)
            .AsQueryable();

        var myItems = await myQuery
            .Skip((request.MyPageNumber - 1) * request.MyPageSize)
            .Take(request.MyPageSize)
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

        // Pending approvals from direct reports
        var pendingQuery = _context.LeaveRequests
            .Include(lr => lr.Employee).ThenInclude(e => e.Department)
            .Include(lr => lr.Employee).ThenInclude(e => e.JobTitle)
            .Include(lr => lr.Employee).ThenInclude(e => e.Branch)
            .Include(lr => lr.LeavePolicy)
            .Include(lr => lr.LeaveStatus)
            .Include(lr => lr.LeaveType)
            .Where(lr => lr.LeaveStatusId == LeaveStatusIds.Pending && lr.Employee.DirectManagerId == currentEmployee.Id)
            .OrderBy(lr => lr.StartDate)
            .AsQueryable();

        var pendingItems = await pendingQuery
            .Skip((request.PendingPageNumber - 1) * request.PendingPageSize)
            .Take(request.PendingPageSize)
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

        var dto = new ManagerLeaveOverviewDto
        {
            MyRequests = myItems,
            PendingApprovals = pendingItems
        };

        return new GenericResponse<ManagerLeaveOverviewDto>
        {
            Success = true,
            Message = "Manager leave overview retrieved successfully",
            Data = dto
        };
    }
}
