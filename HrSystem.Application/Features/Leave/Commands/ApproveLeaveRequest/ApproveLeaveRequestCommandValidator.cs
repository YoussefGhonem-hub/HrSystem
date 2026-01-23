using FluentValidation;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Leave.Commands.ApproveLeaveRequest;

public class ApproveLeaveRequestCommandValidator : AbstractValidator<ApproveLeaveRequestCommand>
{
    private readonly ApplicationDbContext _context;

    public ApproveLeaveRequestCommandValidator(ApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.LeaveRequestId)
            .NotEmpty()
            .WithMessage("Leave request ID is required")
            .NotEqual(Guid.Empty)
            .WithMessage("Leave request ID must be a valid GUID")
            .MustAsync(LeaveRequestExists)
            .WithMessage("Leave request not found")
            .DependentRules(() =>
            {
                RuleFor(x => x.LeaveRequestId)
                    .MustAsync(NotBeRejected)
                    .WithMessage("Cannot approve a rejected leave request")
                    .MustAsync(NotBeAlreadyApproved)
                    .WithMessage("Leave request is already fully approved");
            });

        RuleFor(x => x)
            .MustAsync(UserMustBeLinkedToEmployee)
            .WithMessage("Current user is not linked to an employee")
            .DependentRules(() =>
            {
                RuleFor(x => x.LeaveRequestId)
                    .MustAsync(UserMustHavePermissionToApprove)
                    .WithMessage("You are not authorized to approve this leave request at this stage");
            });

        RuleFor(x => x.Comments)
            .MaximumLength(500)
            .WithMessage("Comments cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Comments));
    }

    private async Task<bool> LeaveRequestExists(Guid leaveRequestId, CancellationToken cancellationToken)
    {
        return await _context.LeaveRequests.AnyAsync(lr => lr.Id == leaveRequestId, cancellationToken);
    }

    private async Task<bool> NotBeRejected(Guid leaveRequestId, CancellationToken cancellationToken)
    {
        var leaveRequest = await _context.LeaveRequests
            .FirstOrDefaultAsync(lr => lr.Id == leaveRequestId, cancellationToken);

        return leaveRequest?.LeaveStatusId != LeaveStatusIds.Rejected;
    }

    private async Task<bool> NotBeAlreadyApproved(Guid leaveRequestId, CancellationToken cancellationToken)
    {
        var leaveRequest = await _context.LeaveRequests
            .FirstOrDefaultAsync(lr => lr.Id == leaveRequestId, cancellationToken);

        return leaveRequest?.LeaveStatusId != LeaveStatusIds.Approved;
    }

    private async Task<bool> UserMustBeLinkedToEmployee(ApproveLeaveRequestCommand command, CancellationToken cancellationToken)
    {
        var currentEmployee = await _context.Employees
            .FirstOrDefaultAsync(e => e.UserId.ToString() == CurrentUser.UserId, cancellationToken);

        return currentEmployee != null;
    }

    private async Task<bool> UserMustHavePermissionToApprove(ApproveLeaveRequestCommand command, Guid leaveRequestId, CancellationToken cancellationToken)
    {
        var leaveRequest = await _context.LeaveRequests
            .Include(lr => lr.Employee)
            .FirstOrDefaultAsync(lr => lr.Id == leaveRequestId, cancellationToken);

        if (leaveRequest == null)
            return false;

        var currentEmployee = await _context.Employees
            .FirstOrDefaultAsync(e => e.UserId.ToString() == CurrentUser.UserId, cancellationToken);

        if (currentEmployee == null)
            return false;

        bool isDirectManager = leaveRequest.Employee.DirectManagerId == currentEmployee.Id;
        bool isHRManager = CurrentUser.Roles?.Contains("HR Manager") == true ||
                          CurrentUser.Roles?.Contains("Admin") == true;

        // If pending, only direct manager can approve
        if (leaveRequest.LeaveStatusId == LeaveStatusIds.Pending)
        {
            return isDirectManager;
        }

        // If manager approved, only HR can approve
        if (leaveRequest.LeaveStatusId == LeaveStatusIds.ManagerApproved)
        {
            return isHRManager;
        }

        // Invalid status
        return false;
    }
}
