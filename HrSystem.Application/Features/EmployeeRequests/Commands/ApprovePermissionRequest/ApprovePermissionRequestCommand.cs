using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Commands.ApproveVacationRequest;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Commands.ApprovePermissionRequest;

public record ApprovePermissionRequestCommand(
    Guid RequestId,
    bool IsApproved,
    string? Comments,
    ApprovalLevel Level = ApprovalLevel.Manager
) : IRequest<ErrorOr<GenericResponse<EmployeeRequestDto>>>;

public class ApprovePermissionRequestCommandHandler
    : IRequestHandler<ApprovePermissionRequestCommand, ErrorOr<GenericResponse<EmployeeRequestDto>>>
{
    private readonly ApplicationDbContext _context;

    public ApprovePermissionRequestCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<EmployeeRequestDto>>> Handle(
        ApprovePermissionRequestCommand request,
        CancellationToken cancellationToken)
    {
        var employeeRequest = await _context.EmployeeRequests
            .Include(r => r.RequestTypeRef)
            .Include(r => r.PermissionDetail)
                .ThenInclude(p => p!.PermissionType)
            .FirstOrDefaultAsync(r => r.Id == request.RequestId && r.RequestTypeRef != null && r.RequestTypeRef.Code == "Permission", cancellationToken);

        if (employeeRequest == null)
            return Error.NotFound(description: "Permission request not found.");

        if (employeeRequest.PermissionDetail == null)
            return Error.Validation(description: "Permission request details not found.");

        var currentUserId = CurrentUser.EmployeeId;

        if (request.Level == ApprovalLevel.Manager)
        {
            // Manager approval
            if (employeeRequest.Status != EmployeeRequestStatus.Pending)
                return Error.Validation(description: "Request is not in pending status for manager approval.");

            employeeRequest.PermissionDetail.ManagerApprovalDate = DateTime.UtcNow;
            employeeRequest.PermissionDetail.ManagerComments = request.Comments;
            employeeRequest.PermissionDetail.ManagerId = currentUserId;

            if (request.IsApproved)
            {
                // For permission requests, check if permission type requires HR approval
                var permissionType = employeeRequest.PermissionDetail.PermissionType;
                if (permissionType?.RequiresManagerApproval == true)
                {
                    // Move to ManagerApproved status for HR to process
                    employeeRequest.Status = EmployeeRequestStatus.ManagerApproved;
                }
                else
                {
                    // If no HR approval needed, directly approve
                    employeeRequest.Status = EmployeeRequestStatus.Approved;
                    employeeRequest.ProcessedBy = currentUserId;
                    employeeRequest.ProcessedDate = DateTime.UtcNow;
                }
            }
            else
            {
                // Rejected by manager
                employeeRequest.Status = EmployeeRequestStatus.Rejected;
                employeeRequest.RejectionReason = request.Comments;
                employeeRequest.ProcessedBy = currentUserId;
                employeeRequest.ProcessedDate = DateTime.UtcNow;
            }
        }
        else if (request.Level == ApprovalLevel.HR)
        {
            // HR approval
            if (employeeRequest.Status != EmployeeRequestStatus.ManagerApproved)
                return Error.Validation(description: "Request must be approved by manager first before HR approval.");

            if (request.IsApproved)
            {
                employeeRequest.Status = EmployeeRequestStatus.Approved;
                employeeRequest.ProcessedBy = currentUserId;
                employeeRequest.ProcessedDate = DateTime.UtcNow;

                // Calculate leave deduction if permission type deducts from leave
                var permissionType = employeeRequest.PermissionDetail.PermissionType;
                if (permissionType?.DeductsFromLeave == true && permissionType.HoursPerLeaveDay.HasValue)
                {
                    var leaveDeduction = employeeRequest.PermissionDetail.TotalHours / permissionType.HoursPerLeaveDay.Value;
                    employeeRequest.PermissionDetail.LeaveDeduction = leaveDeduction;
                }
            }
            else
            {
                // Rejected by HR
                employeeRequest.Status = EmployeeRequestStatus.Rejected;
                employeeRequest.RejectionReason = request.Comments;
                employeeRequest.ProcessedBy = currentUserId;
                employeeRequest.ProcessedDate = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        var dto = new EmployeeRequestDto
        {
            Id = employeeRequest.Id,
            RequestTypeId = employeeRequest.RequestTypeId,
            RequestTypeName = employeeRequest.RequestTypeRef?.Code ?? "",
            Status = employeeRequest.Status,
            EmployeeId = employeeRequest.EmployeeId,
            BranchId = employeeRequest.BranchId,
            Title = employeeRequest.Title,
            Description = employeeRequest.Description,
            RequestedDate = employeeRequest.RequestedDate,
            StartDate = employeeRequest.StartDate,
            EndDate = employeeRequest.EndDate,
            AttachmentUrl = employeeRequest.AttachmentUrl,
            ProcessedBy = employeeRequest.ProcessedBy,
            ProcessedDate = employeeRequest.ProcessedDate,
            RejectionReason = employeeRequest.RejectionReason,
            PermissionDetail = new PermissionDetailDto
            {
                PermissionTypeId = employeeRequest.PermissionDetail.PermissionTypeId,
                PermissionTypeName = employeeRequest.PermissionDetail.PermissionType?.NameEn,
                PermissionDate = employeeRequest.PermissionDetail.PermissionDate,
                FromTime = employeeRequest.PermissionDetail.FromTime,
                ToTime = employeeRequest.PermissionDetail.ToTime,
                TotalHours = employeeRequest.PermissionDetail.TotalHours,
                Reason = employeeRequest.PermissionDetail.Reason,
                ManagerId = employeeRequest.PermissionDetail.ManagerId,
                ManagerApprovalDate = employeeRequest.PermissionDetail.ManagerApprovalDate,
                ManagerComments = employeeRequest.PermissionDetail.ManagerComments,
                LeaveDeduction = employeeRequest.PermissionDetail.LeaveDeduction
            }
        };

        var actionText = request.IsApproved ? "approved" : "rejected";
        var levelText = request.Level == ApprovalLevel.Manager ? "Manager" : "HR";
        return GenericResponse<EmployeeRequestDto>.SuccessResult(dto, $"Permission request {actionText} by {levelText}");
    }
}
