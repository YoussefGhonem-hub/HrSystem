using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Entities.Leave;
using HrSystem.Domain.Entities.Requests;
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
                var deductionError = await TryDeductLeaveBalanceAsync(employeeRequest, cancellationToken);
                if (deductionError is Error error)
                {
                    return error;
                }

                employeeRequest.Status = EmployeeRequestStatus.Approved;
                employeeRequest.ProcessedBy = currentUserId;
                employeeRequest.ProcessedDate = DateTime.UtcNow;
            }
            else
            {
                // Rejected by HR - restore leave balance if previously deducted
                await TryRestoreLeaveBalanceAsync(employeeRequest, cancellationToken);

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

    private async Task<Error?> TryDeductLeaveBalanceAsync(EmployeeRequest employeeRequest, CancellationToken cancellationToken)
    {
        if (!employeeRequest.StartDate.HasValue || !employeeRequest.EndDate.HasValue)
        {
            return Error.Validation(description: "Vacation request must have both start and end dates before approval.");
        }

        if (employeeRequest.StartDate.Value.Year != employeeRequest.EndDate.Value.Year)
        {
            return Error.Validation(description: "Leave requests spanning multiple calendar years must be split per year before approval.");
        }

        var approvalYear = employeeRequest.StartDate.Value.Year;
        var vacationDetail = employeeRequest.VacationDetail!;
        var requestedDays = vacationDetail.TotalDays;

        var leaveBalance = await _context.EmployeeLeaveBalances
            .FirstOrDefaultAsync(b => b.EmployeeId == employeeRequest.EmployeeId
                                      && b.VacationTypeId == vacationDetail.VacationTypeId
                                      && b.Year == approvalYear,
                cancellationToken);

        if (leaveBalance is null)
        {
            return Error.Validation(description: "Leave balance is not configured for this employee, vacation type, and year.");
        }

        var availableDays = leaveBalance.CalculateAvailableDays();
        if (availableDays < requestedDays)
        {
            return Error.Validation(description: $"Insufficient leave balance. Requested {requestedDays:0.##} day(s) but only {availableDays:0.##} day(s) are available.");
        }

        leaveBalance.UsedDays += requestedDays;

        var transaction = new EmployeeLeaveTransaction
        {
            EmployeeLeaveBalanceId = leaveBalance.Id,
            EmployeeId = leaveBalance.EmployeeId,
            VacationTypeId = leaveBalance.VacationTypeId,
            Year = leaveBalance.Year,
            TransactionType = LeaveTransactionType.Deduction,
            DaysChanged = -requestedDays,
            BalanceAfter = leaveBalance.CalculateAvailableDays(),
            ReferenceType = "VacationRequest",
            ReferenceId = employeeRequest.Id,
            Notes = employeeRequest.Title,
            TenantId = leaveBalance.TenantId,
            BranchId = leaveBalance.BranchId
        };

        await _context.EmployeeLeaveTransactions.AddAsync(transaction, cancellationToken);

        return null;
    }

    private async Task TryRestoreLeaveBalanceAsync(EmployeeRequest employeeRequest, CancellationToken cancellationToken)
    {
        if (!employeeRequest.StartDate.HasValue || !employeeRequest.EndDate.HasValue)
            return;

        var approvalYear = employeeRequest.StartDate.Value.Year;
        var vacationDetail = employeeRequest.VacationDetail!;

        // Check if leave was actually deducted for this request
        var deductionTransaction = await _context.EmployeeLeaveTransactions
            .FirstOrDefaultAsync(t =>
                t.EmployeeId == employeeRequest.EmployeeId &&
                t.ReferenceId == employeeRequest.Id &&
                t.ReferenceType == "VacationRequest" &&
                t.TransactionType == LeaveTransactionType.Deduction,
                cancellationToken);

        if (deductionTransaction == null)
            return; // No deduction was made, nothing to restore

        var leaveBalance = await _context.EmployeeLeaveBalances
            .FirstOrDefaultAsync(b =>
                b.EmployeeId == employeeRequest.EmployeeId &&
                b.VacationTypeId == vacationDetail.VacationTypeId &&
                b.Year == approvalYear,
                cancellationToken);

        if (leaveBalance == null)
            return;

        var daysToRestore = vacationDetail.TotalDays;
        leaveBalance.UsedDays -= daysToRestore;
        if (leaveBalance.UsedDays < 0) leaveBalance.UsedDays = 0;

        var creditTransaction = new EmployeeLeaveTransaction
        {
            EmployeeLeaveBalanceId = leaveBalance.Id,
            EmployeeId = leaveBalance.EmployeeId,
            VacationTypeId = leaveBalance.VacationTypeId,
            Year = leaveBalance.Year,
            TransactionType = LeaveTransactionType.Credit,
            DaysChanged = daysToRestore,
            BalanceAfter = leaveBalance.CalculateAvailableDays(),
            ReferenceType = "VacationRequest",
            ReferenceId = employeeRequest.Id,
            Notes = $"Balance restored - request rejected: {employeeRequest.Title}",
            TenantId = leaveBalance.TenantId,
            BranchId = leaveBalance.BranchId
        };

        await _context.EmployeeLeaveTransactions.AddAsync(creditTransaction, cancellationToken);
    }
}
