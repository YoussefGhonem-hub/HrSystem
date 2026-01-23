using ErrorOr;
using HrSystem.Application.Features.Leave.Queries.GetLeaveRequests;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Leave.Queries.GetLeaveRequestById;

public record GetLeaveRequestByIdQuery(Guid Id) : IRequest<ErrorOr<GenericResponse<LeaveRequestDetailsDto>>>;

public class GetLeaveRequestByIdQueryHandler : IRequestHandler<GetLeaveRequestByIdQuery, ErrorOr<GenericResponse<LeaveRequestDetailsDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetLeaveRequestByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<LeaveRequestDetailsDto>>> Handle(
        GetLeaveRequestByIdQuery request,
        CancellationToken cancellationToken)
    {
        var currentEmployee = await _context.Employees
            .FirstOrDefaultAsync(e => e.UserId.ToString() == CurrentUser.UserId, cancellationToken);

        if (currentEmployee == null)
        {
            return Error.Unauthorized("User.NotLinkedToEmployee", "Current user is not linked to an employee");
        }

        bool isHRManager = CurrentUser.Roles?.Contains(RoleNames.HRManager) == true ||
                           CurrentUser.Roles?.Contains(RoleNames.OrganizationAdmin) == true;

        bool isDepartmentManager = CurrentUser.Roles?.Contains(RoleNames.DepartmentManager) == true;

        bool isEmployee = !isHRManager && !isDepartmentManager;

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
            .Include(lr => lr.Manager)
            .AsQueryable();

        query = query.ApplyRoleBasedFilters(
            currentEmployee.Id,
            isHRManager,
            isDepartmentManager,
            isEmployee);

        var leaveRequest = await query
            .FirstOrDefaultAsync(lr => lr.Id == request.Id, cancellationToken);

        if (leaveRequest == null)
        {
            return Error.NotFound("LeaveRequest.NotFound", "Leave request not found");
        }

        var hrApprover = leaveRequest.HRApprovedBy.HasValue
            ? await _context.Employees
                .Where(e => e.Id == leaveRequest.HRApprovedBy.Value)
                .Select(e => new { e.FullNameEn, e.FullNameAr })
                .FirstOrDefaultAsync(cancellationToken)
            : null;

        var remainingBalance = await _context.LeaveBalances
            .Where(lb => lb.EmployeeId == leaveRequest.EmployeeId &&
                         lb.LeavePolicyId == leaveRequest.LeavePolicyId &&
                         lb.Year == leaveRequest.StartDate.Year)
            .Select(lb => (decimal?)lb.RemainingDays)
            .FirstOrDefaultAsync(cancellationToken);

        if (!remainingBalance.HasValue)
        {
            remainingBalance = await _context.LeaveBalances
                .Where(lb => lb.EmployeeId == leaveRequest.EmployeeId &&
                             lb.LeavePolicyId == leaveRequest.LeavePolicyId)
                .OrderByDescending(lb => lb.Year)
                .Select(lb => (decimal?)lb.RemainingDays)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var dto = new LeaveRequestDetailsDto
        {
            Id = leaveRequest.Id,
            EmployeeId = leaveRequest.EmployeeId,
            EmployeeCode = leaveRequest.Employee.EmployeeCode,
            EmployeeName = leaveRequest.Employee.FullNameEn,
            EmployeeNameAr = leaveRequest.Employee.FullNameAr,
            DepartmentName = leaveRequest.Employee.Department.NameEn,
            JobTitle = leaveRequest.Employee.JobTitle.TitleEn,
            BranchName = leaveRequest.Employee.Branch != null ? leaveRequest.Employee.Branch.NameEn : null,
            LeaveTypeId = leaveRequest.LeaveTypeId,
            LeaveTypeName = leaveRequest.LeaveType.NameEn,
            LeaveTypeNameAr = leaveRequest.LeaveType.NameAr,
            StartDate = leaveRequest.StartDate,
            EndDate = leaveRequest.EndDate,
            TotalDays = leaveRequest.TotalDays,
            Reason = leaveRequest.Reason,
            StatusId = leaveRequest.LeaveStatusId,
            StatusName = leaveRequest.LeaveStatus.NameEn,
            StatusNameAr = leaveRequest.LeaveStatus.NameAr,
            ManagerApprovalDate = leaveRequest.ManagerApprovalDate,
            ManagerComments = leaveRequest.ManagerComments,
            HRApprovalDate = leaveRequest.HRApprovalDate,
            HRComments = leaveRequest.HRComments,
            DocumentUrl = leaveRequest.DocumentUrl,
            DocumentPath = leaveRequest.DocumentPath,
            DocumentFileName = LeaveRequestDetailsDto.GetFileName(leaveRequest.DocumentUrl, leaveRequest.DocumentPath),
            CreatedDate = leaveRequest.CreatedDate,
            RequiresHRApproval = leaveRequest.LeavePolicy.RequiresHRApproval,
            CurrentApprovalLevel = leaveRequest.LeaveStatusId == LeaveStatusIds.Pending ? "Manager" :
                                 leaveRequest.LeaveStatusId == LeaveStatusIds.ManagerApproved ? "HR" : "Completed",
            ManagerId = leaveRequest.ManagerId,
            ManagerName = leaveRequest.Manager?.FullNameEn,
            ManagerNameAr = leaveRequest.Manager?.FullNameAr,
            HRApprovedBy = leaveRequest.HRApprovedBy,
            HRApprovedByName = hrApprover?.FullNameEn,
            HRApprovedByNameAr = hrApprover?.FullNameAr,
            EmergencyContactName = leaveRequest.EmergencyContactName,
            EmergencyContactPhone = leaveRequest.EmergencyContactPhone,
            RemainingBalance = remainingBalance
        };

        dto.ApprovedByName = dto.HRApprovedByName ?? dto.ManagerName;
        dto.ApprovedByNameAr = dto.HRApprovedByNameAr ?? dto.ManagerNameAr;
        dto.ApprovedDate = dto.HRApprovalDate ?? dto.ManagerApprovalDate;

        return new GenericResponse<LeaveRequestDetailsDto>
        {
            Success = true,
            Data = dto,
            Message = "Leave request details retrieved successfully"
        };
    }
}
