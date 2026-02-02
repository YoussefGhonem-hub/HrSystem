using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Commands.ApproveVacationRequest;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Commands.Miscellaneous;

public record ApproveMiscellaneousRequestCommand(
    Guid RequestId,
    bool IsApproved,
    string? Comments,
    ApprovalLevel Level = ApprovalLevel.Manager
) : IRequest<ErrorOr<GenericResponse<EmployeeRequestDto>>>;

public class ApproveMiscellaneousRequestCommandHandler
    : IRequestHandler<ApproveMiscellaneousRequestCommand, ErrorOr<GenericResponse<EmployeeRequestDto>>>
{
    private readonly ApplicationDbContext _context;

    public ApproveMiscellaneousRequestCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<EmployeeRequestDto>>> Handle(
        ApproveMiscellaneousRequestCommand request,
        CancellationToken cancellationToken)
    {
        var employeeRequest = await _context.EmployeeRequests
            .Include(r => r.RequestTypeRef)
            .Include(r => r.MiscellaneousDetail)
                .ThenInclude(m => m!.MiscellaneousType)
            .FirstOrDefaultAsync(r => r.Id == request.RequestId && r.RequestTypeRef != null && r.RequestTypeRef.Code == "Miscellaneous", cancellationToken);

        if (employeeRequest == null)
            return Error.NotFound(description: "Miscellaneous request not found.");

        if (employeeRequest.MiscellaneousDetail == null)
            return Error.Validation(description: "Miscellaneous request details not found.");

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
            ProcessedBy = employeeRequest.ProcessedBy,
            ProcessedDate = employeeRequest.ProcessedDate,
            RejectionReason = employeeRequest.RejectionReason,
            MiscellaneousDetail = new MiscellaneousDetailDto
            {
                MiscellaneousTypeId = employeeRequest.MiscellaneousDetail.MiscellaneousTypeId,
                MiscellaneousTypeName = employeeRequest.MiscellaneousDetail.MiscellaneousType?.NameEn,
                AdditionalNotes = employeeRequest.MiscellaneousDetail.AdditionalNotes,
                ReferenceNumber = employeeRequest.MiscellaneousDetail.ReferenceNumber,
                Priority = employeeRequest.MiscellaneousDetail.Priority,
                ExpectedCompletionDate = employeeRequest.MiscellaneousDetail.ExpectedCompletionDate
            }
        };

        var actionText = request.IsApproved ? "approved" : "rejected";
        var levelText = request.Level == ApprovalLevel.Manager ? "Manager" : "HR";

        return GenericResponse<EmployeeRequestDto>.SuccessResult(dto, $"Miscellaneous request {actionText} by {levelText} successfully.");
    }
}
