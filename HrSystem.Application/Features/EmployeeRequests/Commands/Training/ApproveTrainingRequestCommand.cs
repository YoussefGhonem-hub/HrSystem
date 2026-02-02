using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Commands.ApproveVacationRequest;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Commands.Training;

public record ApproveTrainingRequestCommand(
    Guid RequestId,
    bool IsApproved,
    string? Comments,
    ApprovalLevel Level = ApprovalLevel.Manager
) : IRequest<ErrorOr<GenericResponse<EmployeeRequestDto>>>;

public class ApproveTrainingRequestCommandHandler
    : IRequestHandler<ApproveTrainingRequestCommand, ErrorOr<GenericResponse<EmployeeRequestDto>>>
{
    private readonly ApplicationDbContext _context;

    public ApproveTrainingRequestCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<EmployeeRequestDto>>> Handle(
        ApproveTrainingRequestCommand request,
        CancellationToken cancellationToken)
    {
        var employeeRequest = await _context.EmployeeRequests
            .Include(r => r.RequestTypeRef)
            .Include(r => r.TrainingDetail)
                .ThenInclude(t => t!.TrainingType)
            .FirstOrDefaultAsync(r => r.Id == request.RequestId && r.RequestTypeRef != null && r.RequestTypeRef.Code == "Training", cancellationToken);

        if (employeeRequest == null)
            return Error.NotFound(description: "Training request not found.");

        if (employeeRequest.TrainingDetail == null)
            return Error.Validation(description: "Training request details not found.");

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
            TrainingDetail = new TrainingDetailDto
            {
                TrainingTypeId = employeeRequest.TrainingDetail.TrainingTypeId,
                TrainingTypeName = employeeRequest.TrainingDetail.TrainingType?.NameEn,
                TrainingName = employeeRequest.TrainingDetail.TrainingName,
                TrainingProvider = employeeRequest.TrainingDetail.TrainingProvider,
                TrainingLocation = employeeRequest.TrainingDetail.TrainingLocation,
                TrainingStartDate = employeeRequest.TrainingDetail.TrainingStartDate,
                TrainingEndDate = employeeRequest.TrainingDetail.TrainingEndDate,
                DurationDays = employeeRequest.TrainingDetail.DurationDays,
                EstimatedCost = employeeRequest.TrainingDetail.EstimatedCost,
                ApprovedBudget = employeeRequest.TrainingDetail.ApprovedBudget,
                Currency = employeeRequest.TrainingDetail.Currency,
                Objectives = employeeRequest.TrainingDetail.Objectives,
                ExpectedOutcome = employeeRequest.TrainingDetail.ExpectedOutcome
            }
        };

        var actionText = request.IsApproved ? "approved" : "rejected";
        var levelText = request.Level == ApprovalLevel.Manager ? "Manager" : "HR";

        return GenericResponse<EmployeeRequestDto>.SuccessResult(dto, $"Training request {actionText} by {levelText} successfully.");
    }
}
