using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Commands.ApproveVacationRequest;

public record ApproveVacationRequestCommand(
    Guid RequestId,
    bool IsApproved,
    string? Comments,
    ApprovalLevel Level = ApprovalLevel.Manager
) : IRequest<ErrorOr<GenericResponse<EmployeeRequestDto>>>;

public enum ApprovalLevel
{
    Manager = 1,
    HR = 2
}

public class ApproveVacationRequestCommandHandler
    : IRequestHandler<ApproveVacationRequestCommand, ErrorOr<GenericResponse<EmployeeRequestDto>>>
{
    private readonly ApplicationDbContext _context;

    public ApproveVacationRequestCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<EmployeeRequestDto>>> Handle(
        ApproveVacationRequestCommand request,
        CancellationToken cancellationToken)
    {
        var employeeRequest = await _context.EmployeeRequests
            .Include(r => r.RequestTypeRef)
            .Include(r => r.VacationDetail)
                .ThenInclude(v => v!.VacationType)
            .FirstOrDefaultAsync(r => r.Id == request.RequestId && r.RequestTypeRef != null && r.RequestTypeRef.Code == "Vacation", cancellationToken);

        if (employeeRequest == null)
            return Error.NotFound(description: "Vacation request not found.");

        if (employeeRequest.VacationDetail == null)
            return Error.Validation(description: "Vacation request details not found.");

        var currentUserId = CurrentUser.EmployeeId;

        if (request.Level == ApprovalLevel.Manager)
        {
            // Manager approval
            if (employeeRequest.Status != EmployeeRequestStatus.Pending)
                return Error.Validation(description: "Request is not in pending status for manager approval.");

            employeeRequest.VacationDetail.ManagerApprovalDate = DateTime.UtcNow;
            employeeRequest.VacationDetail.ManagerComments = request.Comments;

            if (request.IsApproved)
            {
                employeeRequest.Status = EmployeeRequestStatus.ManagerApproved;
            }
            else
            {
                // Rejected by manager
                employeeRequest.Status = EmployeeRequestStatus.Rejected;
                employeeRequest.VacationDetail.RejectionReason = request.Comments;
                employeeRequest.RejectionReason = request.Comments;
                employeeRequest.ProcessedDate = DateTime.UtcNow;
            }
        }
        else if (request.Level == ApprovalLevel.HR)
        {
            // HR approval
            if (employeeRequest.Status != EmployeeRequestStatus.ManagerApproved)
                return Error.Validation(description: "Request must be approved by manager first before HR approval.");

            employeeRequest.VacationDetail.HRApprovalDate = DateTime.UtcNow;
            employeeRequest.VacationDetail.HRApprovedBy = currentUserId;
            employeeRequest.VacationDetail.HRComments = request.Comments;

            if (request.IsApproved)
            {
                employeeRequest.Status = EmployeeRequestStatus.Approved;
                employeeRequest.ProcessedBy = currentUserId;
                employeeRequest.ProcessedDate = DateTime.UtcNow;
            }
            else
            {
                // Rejected by HR
                employeeRequest.Status = EmployeeRequestStatus.Rejected;
                employeeRequest.VacationDetail.RejectionReason = request.Comments;
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
            VacationDetail = new VacationDetailDto
            {
                VacationTypeId = employeeRequest.VacationDetail.VacationTypeId,
                VacationTypeName = employeeRequest.VacationDetail.VacationType?.NameEn,
                TotalDays = employeeRequest.VacationDetail.TotalDays,
                ManagerId = employeeRequest.VacationDetail.ManagerId,
                ManagerApprovalDate = employeeRequest.VacationDetail.ManagerApprovalDate,
                ManagerComments = employeeRequest.VacationDetail.ManagerComments,
                HRApprovedBy = employeeRequest.VacationDetail.HRApprovedBy,
                HRApprovalDate = employeeRequest.VacationDetail.HRApprovalDate,
                HRComments = employeeRequest.VacationDetail.HRComments,
                EmergencyContactName = employeeRequest.VacationDetail.EmergencyContactName,
                EmergencyContactPhone = employeeRequest.VacationDetail.EmergencyContactPhone
            }
        };

        var actionText = request.IsApproved ? "approved" : "rejected";
        var levelText = request.Level == ApprovalLevel.Manager ? "Manager" : "HR";

        return GenericResponse<EmployeeRequestDto>.SuccessResult(dto, $"Vacation request {actionText} by {levelText} successfully.");
    }
}
