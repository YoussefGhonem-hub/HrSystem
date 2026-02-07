using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Commands.CancelRequest;

/// <summary>
/// Allows an employee to cancel their own request.
/// Only requests in Draft or Pending status may be cancelled.
/// If the request is a Vacation that has already been HR-approved, leave balance is NOT restored here
/// because cancellation is only allowed before full approval.
/// </summary>
public record CancelRequestCommand(
    Guid RequestId,
    string? CancellationReason
) : IRequest<ErrorOr<GenericResponse<EmployeeRequestDto>>>;

public class CancelRequestCommandHandler
    : IRequestHandler<CancelRequestCommand, ErrorOr<GenericResponse<EmployeeRequestDto>>>
{
    private readonly ApplicationDbContext _context;

    public CancelRequestCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<EmployeeRequestDto>>> Handle(
        CancelRequestCommand request,
        CancellationToken cancellationToken)
    {
        var employeeId = CurrentUser.EmployeeId;
        if (!employeeId.HasValue)
            return Error.Forbidden(description: "Employee context is required.");

        var employeeRequest = await _context.EmployeeRequests
            .Include(r => r.RequestTypeRef)
            .Include(r => r.Employee)
            .Include(r => r.VacationDetail).ThenInclude(v => v!.VacationType)
            .Include(r => r.PermissionDetail).ThenInclude(p => p!.PermissionType)
            .Include(r => r.TrainingDetail).ThenInclude(t => t!.TrainingType)
            .Include(r => r.OvertimeDetail).ThenInclude(o => o!.OvertimeType)
            .Include(r => r.MiscellaneousDetail).ThenInclude(m => m!.MiscellaneousType)
            .Include(r => r.PersonalDetail).ThenInclude(p => p!.PersonalType)
            .Include(r => r.FeedbackDetail).ThenInclude(f => f!.FeedbackType)
            .FirstOrDefaultAsync(r => r.Id == request.RequestId, cancellationToken);

        if (employeeRequest == null)
            return Error.NotFound(description: "Request not found.");

        // Only the owner can cancel their request
        if (employeeRequest.EmployeeId != employeeId.Value)
            return Error.Forbidden(description: "You can only cancel your own requests.");

        // Only Draft or Pending requests can be cancelled
        if (employeeRequest.Status != EmployeeRequestStatus.Draft &&
            employeeRequest.Status != EmployeeRequestStatus.Pending)
        {
            return Error.Validation(description:
                $"Cannot cancel a request in '{employeeRequest.Status}' status. Only Draft or Pending requests may be cancelled.");
        }

        employeeRequest.Status = EmployeeRequestStatus.Cancelled;
        employeeRequest.RejectionReason = request.CancellationReason;
        employeeRequest.ProcessedBy = employeeId;
        employeeRequest.ProcessedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var dto = MapToDto(employeeRequest);
        return GenericResponse<EmployeeRequestDto>.SuccessResult(dto, "Request cancelled successfully.");
    }

    private static EmployeeRequestDto MapToDto(Domain.Entities.Requests.EmployeeRequest r) => new()
    {
        Id = r.Id,
        RequestTypeId = r.RequestTypeId,
        RequestTypeName = r.RequestTypeRef?.Code ?? "",
        Status = r.Status,
        EmployeeId = r.EmployeeId,
        EmployeeName = r.Employee?.FullNameEn,
        BranchId = r.BranchId,
        Title = r.Title,
        Description = r.Description,
        RequestedDate = r.RequestedDate,
        StartDate = r.StartDate,
        EndDate = r.EndDate,
        AttachmentUrl = r.AttachmentUrl,
        ManagerComments = r.ManagerComments,
        RejectionReason = r.RejectionReason,
        ApprovedBy = r.ApprovedBy,
        ApprovedDate = r.ApprovedDate,
        ProcessedBy = r.ProcessedBy,
        ProcessedDate = r.ProcessedDate,
        VacationDetail = r.VacationDetail != null ? new VacationDetailDto
        {
            VacationTypeId = r.VacationDetail.VacationTypeId,
            VacationTypeName = r.VacationDetail.VacationType?.NameEn,
            TotalDays = r.VacationDetail.TotalDays,
            ManagerId = r.VacationDetail.ManagerId,
            ManagerApprovalDate = r.VacationDetail.ManagerApprovalDate,
            ManagerComments = r.VacationDetail.ManagerComments,
            HRApprovedBy = r.VacationDetail.HRApprovedBy,
            HRApprovalDate = r.VacationDetail.HRApprovalDate,
            HRComments = r.VacationDetail.HRComments,
            EmergencyContactName = r.VacationDetail.EmergencyContactName,
            EmergencyContactPhone = r.VacationDetail.EmergencyContactPhone
        } : null,
        PermissionDetail = r.PermissionDetail != null ? new PermissionDetailDto
        {
            PermissionTypeId = r.PermissionDetail.PermissionTypeId,
            PermissionTypeName = r.PermissionDetail.PermissionType?.NameEn,
            PermissionDate = r.PermissionDetail.PermissionDate,
            FromTime = r.PermissionDetail.FromTime,
            ToTime = r.PermissionDetail.ToTime,
            TotalHours = r.PermissionDetail.TotalHours,
            Reason = r.PermissionDetail.Reason,
            ManagerId = r.PermissionDetail.ManagerId,
            ManagerApprovalDate = r.PermissionDetail.ManagerApprovalDate,
            ManagerComments = r.PermissionDetail.ManagerComments,
            LeaveDeduction = r.PermissionDetail.LeaveDeduction
        } : null,
        TrainingDetail = r.TrainingDetail != null ? new TrainingDetailDto
        {
            TrainingTypeId = r.TrainingDetail.TrainingTypeId,
            TrainingTypeName = r.TrainingDetail.TrainingType?.NameEn,
            TrainingName = r.TrainingDetail.TrainingName,
            TrainingProvider = r.TrainingDetail.TrainingProvider,
            TrainingLocation = r.TrainingDetail.TrainingLocation,
            TrainingStartDate = r.TrainingDetail.TrainingStartDate,
            TrainingEndDate = r.TrainingDetail.TrainingEndDate,
            DurationDays = r.TrainingDetail.DurationDays,
            EstimatedCost = r.TrainingDetail.EstimatedCost,
            ApprovedBudget = r.TrainingDetail.ApprovedBudget,
            Currency = r.TrainingDetail.Currency,
            Objectives = r.TrainingDetail.Objectives,
            ExpectedOutcome = r.TrainingDetail.ExpectedOutcome,
            CertificationObtained = r.TrainingDetail.CertificationObtained,
            CertificateUrl = r.TrainingDetail.CertificateUrl
        } : null,
        OvertimeDetail = r.OvertimeDetail != null ? new OvertimeDetailDto
        {
            OvertimeTypeId = r.OvertimeDetail.OvertimeTypeId,
            OvertimeTypeName = r.OvertimeDetail.OvertimeType?.NameEn,
            OvertimeDate = r.OvertimeDetail.OvertimeDate,
            PlannedHours = r.OvertimeDetail.PlannedHours,
            ActualHours = r.OvertimeDetail.ActualHours,
            Multiplier = r.OvertimeDetail.Multiplier,
            ProjectCode = r.OvertimeDetail.ProjectCode,
            TaskDescription = r.OvertimeDetail.TaskDescription,
            ApprovedBy = r.OvertimeDetail.ApprovedBy,
            ApprovedDate = r.OvertimeDetail.ApprovedDate,
            ApprovalNotes = r.OvertimeDetail.ApprovalNotes
        } : null,
        MiscellaneousDetail = r.MiscellaneousDetail != null ? new MiscellaneousDetailDto
        {
            MiscellaneousTypeId = r.MiscellaneousDetail.MiscellaneousTypeId,
            MiscellaneousTypeName = r.MiscellaneousDetail.MiscellaneousType?.NameEn,
            AdditionalNotes = r.MiscellaneousDetail.AdditionalNotes,
            ReferenceNumber = r.MiscellaneousDetail.ReferenceNumber,
            Priority = r.MiscellaneousDetail.Priority,
            ExpectedCompletionDate = r.MiscellaneousDetail.ExpectedCompletionDate
        } : null,
        PersonalDetail = r.PersonalDetail != null ? new PersonalDetailDto
        {
            PersonalTypeId = r.PersonalDetail.PersonalTypeId,
            PersonalTypeName = r.PersonalDetail.PersonalType?.NameEn,
            Reason = r.PersonalDetail.Reason,
            IsUrgent = r.PersonalDetail.IsUrgent,
            RequiresConfidentiality = r.PersonalDetail.RequiresConfidentiality
        } : null,
        FeedbackDetail = r.FeedbackDetail != null ? new FeedbackDetailDto
        {
            FeedbackTypeId = r.FeedbackDetail.FeedbackTypeId,
            FeedbackTypeName = r.FeedbackDetail.FeedbackType?.NameEn,
            FeedbackContent = r.FeedbackDetail.FeedbackContent,
            IsAnonymous = r.FeedbackDetail.IsAnonymous,
            Rating = r.FeedbackDetail.Rating,
            TargetDepartment = r.FeedbackDetail.TargetDepartment,
            TargetPerson = r.FeedbackDetail.TargetPerson,
            SuggestedImprovement = r.FeedbackDetail.SuggestedImprovement,
            ResponseRequired = r.FeedbackDetail.ResponseRequired,
            ResponseContent = r.FeedbackDetail.ResponseContent,
            ResponseDate = r.FeedbackDetail.ResponseDate
        } : null
    };
}
