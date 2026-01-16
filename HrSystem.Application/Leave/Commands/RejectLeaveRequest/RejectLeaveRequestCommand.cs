using ErrorOr;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Leave.Commands.RejectLeaveRequest;

/// <summary>
/// Command to reject a leave request
/// Can be rejected by direct manager or HR manager
/// </summary>
public record RejectLeaveRequestCommand : IRequest<ErrorOr<GenericResponse>>
{
    public Guid LeaveRequestId { get; init; }
    public string RejectionReason { get; init; } = string.Empty;
}

public class RejectLeaveRequestCommandHandler : IRequestHandler<RejectLeaveRequestCommand, ErrorOr<GenericResponse>>
{
    private readonly ApplicationDbContext _context;

    public RejectLeaveRequestCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse>> Handle(RejectLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        // All validations are handled by the validator class
        var leaveRequest = await _context.LeaveRequests
            .Include(lr => lr.Employee)
            .FirstOrDefaultAsync(lr => lr.Id == request.LeaveRequestId, cancellationToken);

        // Get current user's employee record
        var currentEmployee = await _context.Employees
            .FirstOrDefaultAsync(e => e.UserId.ToString() == CurrentUser.UserId, cancellationToken);

        // Check authorization
        bool isDirectManager = leaveRequest!.Employee.DirectManagerId == currentEmployee!.Id;
        bool isHRManager = CurrentUser.Roles?.Contains("HR Manager") == true ||
                          CurrentUser.Roles?.Contains("Admin") == true;

        // Reject the leave request
        leaveRequest.Status = LeaveStatus.Rejected;
        leaveRequest.RejectionReason = request.RejectionReason;

        // Track who rejected it
        if (isDirectManager && leaveRequest.Status == LeaveStatus.Pending)
        {
            leaveRequest.ManagerId = currentEmployee.Id;
            leaveRequest.ManagerApprovalDate = DateTime.UtcNow;
            leaveRequest.ManagerComments = $"Rejected: {request.RejectionReason}";
        }
        else if (isHRManager)
        {
            leaveRequest.HRApprovedBy = currentEmployee.Id;
            leaveRequest.HRApprovalDate = DateTime.UtcNow;
            leaveRequest.HRComments = $"Rejected: {request.RejectionReason}";
        }

        await _context.SaveChangesAsync(cancellationToken);
        return GenericResponse.SuccessResult("Leave request rejected successfully");
    }
}
