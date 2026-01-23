using FluentValidation;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Leave.Commands.RejectLeaveRequest;

public class RejectLeaveRequestCommandValidator : AbstractValidator<RejectLeaveRequestCommand>
{
    private readonly ApplicationDbContext _context;

    public RejectLeaveRequestCommandValidator(ApplicationDbContext context)
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
                    .MustAsync(NotBeAlreadyApproved)
                    .WithMessage("Cannot reject an approved leave request")
                    .MustAsync(NotBeAlreadyRejected)
                    .WithMessage("Leave request is already rejected");
            });

        RuleFor(x => x.RejectionReason)
            .NotEmpty()
            .WithMessage("Rejection reason is required")
            .MinimumLength(10)
            .WithMessage("Rejection reason must be at least 10 characters")
            .MaximumLength(500)
            .WithMessage("Rejection reason cannot exceed 500 characters");

        RuleFor(x => x)
            .MustAsync(UserMustBeLinkedToEmployee)
            .WithMessage("Current user is not linked to an employee")
            .DependentRules(() =>
            {
                RuleFor(x => x.LeaveRequestId)
                    .MustAsync(UserMustHavePermissionToReject)
                    .WithMessage("You are not authorized to reject this leave request");
            });
    }

    private async Task<bool> LeaveRequestExists(Guid leaveRequestId, CancellationToken cancellationToken)
    {
        return await _context.LeaveRequests.AnyAsync(lr => lr.Id == leaveRequestId, cancellationToken);
    }

    private async Task<bool> NotBeAlreadyApproved(Guid leaveRequestId, CancellationToken cancellationToken)
    {
        var leaveRequest = await _context.LeaveRequests
            .FirstOrDefaultAsync(lr => lr.Id == leaveRequestId, cancellationToken);

        return leaveRequest?.LeaveStatusId != LeaveStatusIds.Approved;
    }

    private async Task<bool> NotBeAlreadyRejected(Guid leaveRequestId, CancellationToken cancellationToken)
    {
        var leaveRequest = await _context.LeaveRequests
            .FirstOrDefaultAsync(lr => lr.Id == leaveRequestId, cancellationToken);

        return leaveRequest?.LeaveStatusId != LeaveStatusIds.Rejected;
    }

    private async Task<bool> UserMustBeLinkedToEmployee(RejectLeaveRequestCommand command, CancellationToken cancellationToken)
    {
        var currentEmployee = await _context.Employees
            .FirstOrDefaultAsync(e => e.UserId.ToString() == CurrentUser.UserId, cancellationToken);

        return currentEmployee != null;
    }

    private async Task<bool> UserMustHavePermissionToReject(RejectLeaveRequestCommand command, Guid leaveRequestId, CancellationToken cancellationToken)
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

        return isDirectManager || isHRManager;
    }
}
