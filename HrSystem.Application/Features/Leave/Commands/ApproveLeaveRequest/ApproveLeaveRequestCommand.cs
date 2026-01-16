using ErrorOr;
using HrSystem.Application.Common;
using HrSystem.Domain.Entities.Leave;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Leave.Commands.ApproveLeaveRequest;

/// <summary>
/// Command to approve a leave request with multi-level approval workflow
/// Supports hierarchical approval: Direct Manager -> HR Manager
/// </summary>
public record ApproveLeaveRequestCommand : IRequest<ErrorOr<GenericResponse>>
{
    public Guid LeaveRequestId { get; init; }
    public string? Comments { get; init; }
}

public class ApproveLeaveRequestCommandHandler : IRequestHandler<ApproveLeaveRequestCommand, ErrorOr<GenericResponse>>
{
    private readonly ApplicationDbContext _context;

    public ApproveLeaveRequestCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse>> Handle(ApproveLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        // All validations are handled by the validator class
        // Get the leave request with employee details
        var leaveRequest = await _context.LeaveRequests
            .Include(lr => lr.Employee)
            .Include(lr => lr.LeavePolicy)
            .FirstOrDefaultAsync(lr => lr.Id == request.LeaveRequestId, cancellationToken);

        // Get current user's employee record
        var currentEmployee = await _context.Employees
            .FirstOrDefaultAsync(e => e.UserId.ToString() == CurrentUser.UserId, cancellationToken);

        // Check if current user is the direct manager
        bool isDirectManager = leaveRequest!.Employee.DirectManagerId == currentEmployee!.Id;

        // Check if current user has HR role
        bool isHRManager = CurrentUser.Roles?.Contains("HR Manager") == true ||
                          CurrentUser.Roles?.Contains("Admin") == true;

        // **WORKFLOW LOGIC:**

        // Step 1: Direct Manager Approval (Team Leader, Department Manager, etc.)
        if (leaveRequest.Status == LeaveStatus.Pending)
        {
            leaveRequest.Status = LeaveStatus.ManagerApproved;
            leaveRequest.ManagerId = currentEmployee.Id;
            leaveRequest.ManagerApprovalDate = DateTime.UtcNow;
            leaveRequest.ManagerComments = request.Comments;

            // Check if HR approval is required based on leave policy
            if (!leaveRequest.LeavePolicy.RequiresHRApproval)
            {
                // If HR approval not required, mark as fully approved
                leaveRequest.Status = LeaveStatus.Approved;
            }

            await _context.SaveChangesAsync(cancellationToken);

            var message = leaveRequest.Status == LeaveStatus.Approved
                ? "Leave request fully approved"
                : "Leave request approved by manager, pending HR approval";

            return GenericResponse.SuccessResult(message);
        }

        // Step 2: HR Manager Approval
        if (leaveRequest.Status == LeaveStatus.ManagerApproved)
        {
            leaveRequest.Status = LeaveStatus.Approved;
            leaveRequest.HRApprovedBy = currentEmployee.Id;
            leaveRequest.HRApprovalDate = DateTime.UtcNow;
            leaveRequest.HRComments = request.Comments;

            await _context.SaveChangesAsync(cancellationToken);
            return GenericResponse.SuccessResult("Leave request fully approved by HR");
        }

        return Error.Validation("LeaveRequest.InvalidStatus", "Invalid leave request status for approval");
    }
}
