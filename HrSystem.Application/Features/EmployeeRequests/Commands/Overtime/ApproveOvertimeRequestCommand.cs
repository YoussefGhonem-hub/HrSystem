using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Commands.ApproveVacationRequest;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Commands.Overtime;

public record ApproveOvertimeRequestCommand(
    Guid RequestId,
    bool IsApproved,
    string? Comments,
    ApprovalLevel Level = ApprovalLevel.Manager
) : IRequest<ErrorOr<GenericResponse<EmployeeRequestDto>>>;

public class ApproveOvertimeRequestCommandHandler
    : IRequestHandler<ApproveOvertimeRequestCommand, ErrorOr<GenericResponse<EmployeeRequestDto>>>
{
    private readonly ApplicationDbContext _context;

    public ApproveOvertimeRequestCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<EmployeeRequestDto>>> Handle(
        ApproveOvertimeRequestCommand request,
        CancellationToken cancellationToken)
    {
        var employeeRequest = await _context.EmployeeRequests
            .Include(r => r.RequestTypeRef)
            .Include(r => r.OvertimeDetail)
                .ThenInclude(o => o!.OvertimeType)
            .FirstOrDefaultAsync(r => r.Id == request.RequestId
                && r.RequestTypeRef != null
                && r.RequestTypeRef.Code == "OverTime", cancellationToken);

        if (employeeRequest == null)
            return Error.NotFound(description: "Overtime request not found.");

        if (employeeRequest.OvertimeDetail == null)
            return Error.Validation(description: "Overtime request details not found.");

        var currentUserId = CurrentUser.EmployeeId;

        if (request.Level == ApprovalLevel.Manager)
        {
            if (employeeRequest.Status != EmployeeRequestStatus.Pending)
                return Error.Validation(description: "Request is not in pending status for manager approval.");

            employeeRequest.ManagerComments = request.Comments;

            if (request.IsApproved)
            {
                employeeRequest.Status = EmployeeRequestStatus.ManagerApproved;
            }
            else
            {
                employeeRequest.Status = EmployeeRequestStatus.Rejected;
                employeeRequest.RejectionReason = request.Comments;
                employeeRequest.ProcessedDate = DateTime.UtcNow;
            }
        }
        else if (request.Level == ApprovalLevel.HR)
        {
            if (employeeRequest.Status != EmployeeRequestStatus.ManagerApproved)
                return Error.Validation(description: "Request must be approved by manager first before HR approval.");

            if (request.IsApproved)
            {
                employeeRequest.Status = EmployeeRequestStatus.Approved;
                employeeRequest.ApprovedBy = currentUserId;
                employeeRequest.ApprovedDate = DateTime.UtcNow;
                employeeRequest.ProcessedBy = currentUserId;
                employeeRequest.ProcessedDate = DateTime.UtcNow;

                // Also set the overtime detail approval fields
                employeeRequest.OvertimeDetail.ApprovedBy = currentUserId;
                employeeRequest.OvertimeDetail.ApprovedDate = DateTime.UtcNow;
                employeeRequest.OvertimeDetail.ApprovalNotes = request.Comments;
            }
            else
            {
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
            ManagerComments = employeeRequest.ManagerComments,
            RejectionReason = employeeRequest.RejectionReason,
            ApprovedBy = employeeRequest.ApprovedBy,
            ApprovedDate = employeeRequest.ApprovedDate,
            ProcessedBy = employeeRequest.ProcessedBy,
            ProcessedDate = employeeRequest.ProcessedDate,
            OvertimeDetail = new OvertimeDetailDto
            {
                OvertimeTypeId = employeeRequest.OvertimeDetail.OvertimeTypeId,
                OvertimeTypeName = employeeRequest.OvertimeDetail.OvertimeType?.NameEn,
                OvertimeDate = employeeRequest.OvertimeDetail.OvertimeDate,
                PlannedHours = employeeRequest.OvertimeDetail.PlannedHours,
                ActualHours = employeeRequest.OvertimeDetail.ActualHours,
                Multiplier = employeeRequest.OvertimeDetail.Multiplier,
                ProjectCode = employeeRequest.OvertimeDetail.ProjectCode,
                TaskDescription = employeeRequest.OvertimeDetail.TaskDescription,
                ApprovedBy = employeeRequest.OvertimeDetail.ApprovedBy,
                ApprovedDate = employeeRequest.OvertimeDetail.ApprovedDate,
                ApprovalNotes = employeeRequest.OvertimeDetail.ApprovalNotes
            }
        };

        var actionText = request.IsApproved ? "approved" : "rejected";
        var levelText = request.Level == ApprovalLevel.Manager ? "Manager" : "HR";

        return GenericResponse<EmployeeRequestDto>.SuccessResult(dto, $"Overtime request {actionText} by {levelText} successfully.");
    }
}
