using ErrorOr;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Leave.Queries.GetPendingLeaveRequests;

/// <summary>
/// Query to get leave requests pending approval by current user
/// Returns requests where:
/// - User is direct manager and status is Pending
/// - User is HR Manager and status is ManagerApproved
/// </summary>
public record GetPendingLeaveRequestsQuery : IRequest<ErrorOr<GenericResponse<List<LeaveRequestDto>>>>;

public class GetPendingLeaveRequestsQueryHandler : IRequestHandler<GetPendingLeaveRequestsQuery, ErrorOr<GenericResponse<List<LeaveRequestDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetPendingLeaveRequestsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<List<LeaveRequestDto>>>> Handle(GetPendingLeaveRequestsQuery request, CancellationToken cancellationToken)
    {
        // Get current user's employee record
        var currentEmployee = await _context.Employees
            .FirstOrDefaultAsync(e => e.UserId.ToString() == CurrentUser.UserId, cancellationToken);

        if (currentEmployee == null)
            return Error.Unauthorized("User.NotLinkedToEmployee", "Current user is not linked to an employee");

        // Check if user has HR role
        bool isHRManager = CurrentUser.Roles?.Contains("HR Manager") == true ||
                          CurrentUser.Roles?.Contains("Admin") == true;

        var query = _context.LeaveRequests
            .Include(lr => lr.Employee)
                .ThenInclude(e => e.Department)
            .Include(lr => lr.Employee)
                .ThenInclude(e => e.JobTitle)
            .Include(lr => lr.Employee)
                .ThenInclude(e => e.Branch)
            .Include(lr => lr.LeavePolicy)
            .AsQueryable();

        // Filter based on user's role and position
        if (isHRManager)
        {
            // HR Manager sees requests that are:
            // 1. Manager approved and waiting for HR
            // 2. Pending requests for employees without managers (direct reports to HR)
            query = query.Where(lr =>
                lr.Status == LeaveStatus.ManagerApproved ||
                (lr.Status == LeaveStatus.Pending && lr.Employee.DirectManagerId == null));
        }
        else
        {
            // Regular managers see only pending requests from their direct reports
            query = query.Where(lr =>
                lr.Status == LeaveStatus.Pending &&
                lr.Employee.DirectManagerId == currentEmployee.Id);
        }

        var leaveRequests = await query
            .OrderBy(lr => lr.StartDate)
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
                LeaveType = lr.LeaveType,
                LeaveTypeName = lr.LeaveType.ToString(),
                StartDate = lr.StartDate,
                EndDate = lr.EndDate,
                TotalDays = lr.TotalDays,
                Reason = lr.Reason,
                Status = lr.Status,
                StatusName = lr.Status.ToString(),
                ManagerApprovalDate = lr.ManagerApprovalDate,
                ManagerComments = lr.ManagerComments,
                HRApprovalDate = lr.HRApprovalDate,
                HRComments = lr.HRComments,
                DocumentUrl = lr.DocumentUrl,
                CreatedDate = lr.CreatedDate,
                RequiresHRApproval = lr.LeavePolicy.RequiresHRApproval,
                CurrentApprovalLevel = lr.Status == LeaveStatus.Pending ? "Manager" : "HR"
            })
            .ToListAsync(cancellationToken);

        return GenericResponse<List<LeaveRequestDto>>.SuccessResult(leaveRequests, "Leave requests retrieved successfully");
    }
}

public class LeaveRequestDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeNameAr { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string? BranchName { get; set; }
    public LeaveType LeaveType { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalDays { get; set; }
    public string Reason { get; set; } = string.Empty;
    public LeaveStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateTime? ManagerApprovalDate { get; set; }
    public string? ManagerComments { get; set; }
    public DateTime? HRApprovalDate { get; set; }
    public string? HRComments { get; set; }
    public string? DocumentUrl { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public bool RequiresHRApproval { get; set; }
    public string CurrentApprovalLevel { get; set; } = string.Empty;
}
