using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Commands.ApproveVacationRequest;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Entities.Leave;
using HrSystem.Domain.Entities.Payroll;
using HrSystem.Domain.Entities.Requests;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Commands.ApproveRequest;

/// <summary>
/// Unified command to approve or reject any employee request.
/// Automatically detects the request type and applies type-specific logic.
/// The approval level (Manager or HR) is determined from the caller's role.
/// </summary>
public record ApproveRequestCommand(
    Guid RequestId,
    bool IsApproved,
    string? Comments,
    // Loan-specific fields (used when HR approves a Personal/Loan request)
    decimal? LoanAmount = null,
    int? LoanInstallmentMonths = null,
    decimal? LoanMonthlyDeduction = null,
    DateTime? LoanStartDate = null
) : IRequest<ErrorOr<GenericResponse<EmployeeRequestDto>>>;
public class ApproveRequestCommandHandler
    : IRequestHandler<ApproveRequestCommand, ErrorOr<GenericResponse<EmployeeRequestDto>>>
{
    private readonly ApplicationDbContext _context;

    public ApproveRequestCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<EmployeeRequestDto>>> Handle(
        ApproveRequestCommand request,
        CancellationToken cancellationToken)
    {
        // Load the request with ALL possible detail navigations
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

        // Determine approval level from caller's role
        var roles = CurrentUser.Roles
            .Select(RoleNames.Normalize)
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .ToArray();
        var currentUserId = CurrentUser.Id;
        var currentEmployeeId = CurrentUser.EmployeeId;

        bool isHR = roles.Any(r => string.Equals(r, RoleNames.HRManager, StringComparison.OrdinalIgnoreCase))
            || roles.Any(r => string.Equals(r, RoleNames.HRSpecialist, StringComparison.OrdinalIgnoreCase))
            || roles.Any(r => string.Equals(r, RoleNames.OrganizationAdmin, StringComparison.OrdinalIgnoreCase));

        bool isManager = roles.Any(r => string.Equals(r, RoleNames.DepartmentManager, StringComparison.OrdinalIgnoreCase));

        ApprovalLevel level;

        // Check if the approver is the employee's direct manager
        bool isDirectManager = currentEmployeeId.HasValue
            && employeeRequest.Employee.DirectManagerId == currentEmployeeId.Value;

        if (employeeRequest.Status == EmployeeRequestStatus.Pending && isHR && employeeRequest.Employee.DirectManagerId == null)
        {
            // Employee has no direct manager → HR handles full approval directly
            level = ApprovalLevel.HR;
        }
        else if (employeeRequest.Status == EmployeeRequestStatus.Pending && isDirectManager && isHR)
        {
            // Direct manager is also an HR member → single approval covers both levels
            level = ApprovalLevel.HR;
        }
        else if (employeeRequest.Status == EmployeeRequestStatus.Pending && isHR && !RequiresManagerApproval(employeeRequest))
        {
            // Request type does not require manager approval (e.g. Personal/Loan) → HR handles directly
            level = ApprovalLevel.HR;
        }
        else if (employeeRequest.Status == EmployeeRequestStatus.Pending && (isManager || isHR))
        {
            level = ApprovalLevel.Manager;
        }
        else if (employeeRequest.Status == EmployeeRequestStatus.ManagerApproved && isHR)
        {
            level = ApprovalLevel.HR;
        }
        else if (employeeRequest.Status == EmployeeRequestStatus.Pending && !isManager && !isHR)
        {
            return Error.Forbidden(description: "You do not have permission to approve this request.");
        }
        else if (employeeRequest.Status == EmployeeRequestStatus.ManagerApproved && !isHR)
        {
            return Error.Forbidden(description: "Only HR can approve requests at this stage.");
        }
        else
        {
            return Error.Validation(description: $"Request cannot be approved/rejected in its current status: {employeeRequest.Status}.");
        }

        var requestTypeCode = employeeRequest.RequestTypeRef?.Code ?? "";

        // Apply type-specific approval logic
        if (level == ApprovalLevel.Manager)
        {
            return await HandleManagerApproval(employeeRequest, requestTypeCode, request, currentUserId, currentEmployeeId, cancellationToken);
        }
        else
        {
            return await HandleHRApproval(employeeRequest, requestTypeCode, request, currentUserId, currentEmployeeId, isDirectManager, cancellationToken);
        }
    }

    private async Task<ErrorOr<GenericResponse<EmployeeRequestDto>>> HandleManagerApproval(
        EmployeeRequest employeeRequest,
        string requestTypeCode,
        ApproveRequestCommand request,
        Guid? currentUserId,
        Guid? currentEmployeeId,
        CancellationToken cancellationToken)
    {
        employeeRequest.ManagerComments = request.Comments;

        // Type-specific manager approval fields
        if (requestTypeCode == "Vacation" && employeeRequest.VacationDetail != null)
        {
            employeeRequest.VacationDetail.ManagerApprovalDate = DateTime.UtcNow;
            employeeRequest.VacationDetail.ManagerComments = request.Comments;
        }
        else if (requestTypeCode == "Permission" && employeeRequest.PermissionDetail != null)
        {
            employeeRequest.PermissionDetail.ManagerApprovalDate = DateTime.UtcNow;
            employeeRequest.PermissionDetail.ManagerComments = request.Comments;
            employeeRequest.PermissionDetail.ManagerId = currentEmployeeId;
        }

        if (request.IsApproved)
        {
            employeeRequest.Status = EmployeeRequestStatus.ManagerApproved;
        }
        else
        {
            employeeRequest.Status = EmployeeRequestStatus.Rejected;
            employeeRequest.RejectionReason = request.Comments;
            employeeRequest.ProcessedBy = currentUserId;
            employeeRequest.ProcessedDate = DateTime.UtcNow;

            if (requestTypeCode == "Vacation" && employeeRequest.VacationDetail != null)
                employeeRequest.VacationDetail.RejectionReason = request.Comments;
        }

        await _context.SaveChangesAsync(cancellationToken);

        var dto = MapToDto(employeeRequest);
        var actionText = request.IsApproved ? "approved" : "rejected";
        return GenericResponse<EmployeeRequestDto>.SuccessResult(dto, $"Request {actionText} by manager successfully.");
    }

    private async Task<ErrorOr<GenericResponse<EmployeeRequestDto>>> HandleHRApproval(
        EmployeeRequest employeeRequest,
        string requestTypeCode,
        ApproveRequestCommand request,
        Guid? currentUserId,
        Guid? currentEmployeeId,
        bool isDirectManager,
        CancellationToken cancellationToken)
    {
        // When the direct manager is also HR, fill in manager approval fields too
        if (isDirectManager && employeeRequest.Status == EmployeeRequestStatus.Pending)
        {
            employeeRequest.ManagerComments = request.Comments;

            if (requestTypeCode == "Vacation" && employeeRequest.VacationDetail != null)
            {
                employeeRequest.VacationDetail.ManagerApprovalDate = DateTime.UtcNow;
                employeeRequest.VacationDetail.ManagerComments = request.Comments;
            }
            else if (requestTypeCode == "Permission" && employeeRequest.PermissionDetail != null)
            {
                employeeRequest.PermissionDetail.ManagerApprovalDate = DateTime.UtcNow;
                employeeRequest.PermissionDetail.ManagerComments = request.Comments;
                employeeRequest.PermissionDetail.ManagerId = currentEmployeeId;
            }
        }

        if (request.IsApproved)
        {
            // Vacation-specific: deduct leave balance
            if (requestTypeCode == "Vacation" && employeeRequest.VacationDetail != null)
            {
                employeeRequest.VacationDetail.HRApprovalDate = DateTime.UtcNow;
                employeeRequest.VacationDetail.HRApprovedBy = currentEmployeeId;
                employeeRequest.VacationDetail.HRComments = request.Comments;

                var deductionError = await TryDeductLeaveBalanceAsync(employeeRequest, cancellationToken);
                if (deductionError is Error error)
                    return error;
            }

            // Personal/Loan-specific: create a Loan record on HR approval
            if (requestTypeCode == "Personal" && employeeRequest.PersonalDetail != null)
            {
                var loanError = await TryCreateLoanAsync(employeeRequest, request, cancellationToken);
                if (loanError is Error loanErr)
                    return loanErr;
            }

            if (requestTypeCode == "AttendanceCorrection")
            {
                var correctionError = await TryApplyAttendanceCorrectionAsync(employeeRequest, cancellationToken);
                if (correctionError is Error correctionErr)
                    return correctionErr;
            }

            employeeRequest.Status = EmployeeRequestStatus.Approved;
            employeeRequest.ApprovedBy = currentUserId;
            employeeRequest.ApprovedDate = DateTime.UtcNow;
            employeeRequest.ProcessedBy = currentUserId;
            employeeRequest.ProcessedDate = DateTime.UtcNow;
        }
        else
        {
            // Restore leave balance if vacation was previously approved
            if (requestTypeCode == "Vacation" && employeeRequest.VacationDetail != null)
            {
                await TryRestoreLeaveBalanceAsync(employeeRequest, cancellationToken);
            }

            employeeRequest.Status = EmployeeRequestStatus.Rejected;
            employeeRequest.RejectionReason = request.Comments;
            employeeRequest.ProcessedBy = currentUserId;
            employeeRequest.ProcessedDate = DateTime.UtcNow;

            if (requestTypeCode == "Vacation" && employeeRequest.VacationDetail != null)
            {
                employeeRequest.VacationDetail.HRApprovalDate = DateTime.UtcNow;
                employeeRequest.VacationDetail.HRApprovedBy = currentEmployeeId;
                employeeRequest.VacationDetail.HRComments = request.Comments;
                employeeRequest.VacationDetail.RejectionReason = request.Comments;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        var dto = MapToDto(employeeRequest);
        var actionText = request.IsApproved ? "approved" : "rejected";
        return GenericResponse<EmployeeRequestDto>.SuccessResult(dto, $"Request {actionText} by HR successfully.");
    }

    private async Task<Error?> TryDeductLeaveBalanceAsync(EmployeeRequest employeeRequest, CancellationToken cancellationToken)
    {
        if (!employeeRequest.StartDate.HasValue || !employeeRequest.EndDate.HasValue)
            return Error.Validation(description: "Vacation request must have both start and end dates before approval.");

        if (employeeRequest.StartDate.Value.Year != employeeRequest.EndDate.Value.Year)
            return Error.Validation(description: "Leave requests spanning multiple calendar years must be split per year before approval.");

        var approvalYear = employeeRequest.StartDate.Value.Year;
        var vacationDetail = employeeRequest.VacationDetail!;
        var requestedDays = vacationDetail.TotalDays;

        var leaveBalance = await _context.EmployeeLeaveBalances
            .FirstOrDefaultAsync(b =>
                b.EmployeeId == employeeRequest.EmployeeId &&
                b.VacationTypeId == vacationDetail.VacationTypeId &&
                b.Year == approvalYear,
                cancellationToken);

        if (leaveBalance is null)
            return Error.Validation(description: "Leave balance is not configured for this employee, vacation type, and year.");

        var availableDays = leaveBalance.CalculateAvailableDays();
        if (availableDays < requestedDays)
            return Error.Validation(description: $"Insufficient leave balance. Requested {requestedDays:0.##} day(s) but only {availableDays:0.##} day(s) are available.");

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

    private static EmployeeRequestDto MapToDto(EmployeeRequest r) => new()
    {
        Id = r.Id,
        RequestTypeId = r.RequestTypeId,
        RequestTypeCode = r.RequestTypeRef?.Code ?? "",
        RequestTypeName = r.RequestTypeRef?.NameEn ?? "",
        Status = r.Status,
        EmployeeId = r.EmployeeId,
        EmployeeName = r.Employee?.FullNameEn,
        EmployeeCode = r.Employee?.EmployeeCode,
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

    /// <summary>
    /// Checks whether the request's sub-type requires manager approval.
    /// Returns false for Personal types with RequiresManagerApproval = false (e.g. Loan).
    /// </summary>
    private static bool RequiresManagerApproval(EmployeeRequest employeeRequest)
    {
        if (employeeRequest.PersonalDetail?.PersonalType != null)
            return employeeRequest.PersonalDetail.PersonalType.RequiresManagerApproval;

        // Default: all other request types require manager approval
        return true;
    }

    private async Task<Error?> TryApplyAttendanceCorrectionAsync(
        EmployeeRequest employeeRequest,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(employeeRequest.Description))
            return Error.Validation("AttendanceCorrection.MissingData", "Attendance correction details are missing.");

        if (!TryExtractDescriptionValue(employeeRequest.Description, "AttendanceCorrectionTypeId", out var correctionTypeIdRaw) ||
            !Guid.TryParse(correctionTypeIdRaw, out var correctionTypeId))
        {
            return Error.Validation("AttendanceCorrection.InvalidType", "Attendance correction type is invalid or missing.");
        }

        if (!TryExtractDescriptionValue(employeeRequest.Description, "AttendanceDate", out var attendanceDateRaw) ||
            !DateTime.TryParse(attendanceDateRaw, out var attendanceDate))
        {
            return Error.Validation("AttendanceCorrection.InvalidDate", "Attendance correction date is invalid or missing.");
        }

        if (!TryExtractDescriptionValue(employeeRequest.Description, "CorrectedTime", out var correctedTimeRaw) ||
            !TimeSpan.TryParse(correctedTimeRaw, out var correctedTime))
        {
            return Error.Validation("AttendanceCorrection.InvalidTime", "Attendance correction time is invalid or missing.");
        }

        var correctionType = await _context.AttendanceCorrectionTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == correctionTypeId && t.IsActive && !t.IsDeleted, cancellationToken);

        if (correctionType is null)
            return Error.Validation("AttendanceCorrection.InvalidType", "Attendance correction type was not found.");

        var typeText = $"{correctionType.NameEn} {correctionType.NameAr}".ToLower();
        var applyToCheckIn = typeText.Contains("check-in") || typeText.Contains("check in") || typeText.Contains("حضور");
        var applyToCheckOut = typeText.Contains("check-out") || typeText.Contains("check out") || typeText.Contains("انصراف");

        if (!applyToCheckIn && !applyToCheckOut)
            return Error.Validation("AttendanceCorrection.UnsupportedType", "Attendance correction type must target check-in or check-out.");

        var targetDate = attendanceDate.Date;

        var attendance = await _context.Attendances
            .FirstOrDefaultAsync(a =>
                !a.IsDeleted &&
                !a.IsConfigurationRecord &&
                a.EmployeeId == employeeRequest.EmployeeId &&
                a.Date == targetDate,
                cancellationToken);

        if (attendance is null)
        {
            attendance = new HrSystem.Domain.Entities.Attendance.Attendance
            {
                EmployeeId = employeeRequest.EmployeeId,
                Date = targetDate,
                StatusId = AttendanceStatusIds.Present,
                BranchId = employeeRequest.BranchId,
                TenantId = employeeRequest.TenantId
            };

            attendance.MarkAsCreated(CurrentUser.Id ?? Guid.Empty);
            _context.Attendances.Add(attendance);
        }

        if (applyToCheckIn)
        {
            attendance.CheckInTime = correctedTime;
            attendance.CheckInMethod = AttendanceMethod.Manual;
        }

        if (applyToCheckOut)
        {
            attendance.CheckOutTime = correctedTime;
            attendance.CheckOutMethod = AttendanceMethod.Manual;
        }

        if (attendance.CheckInTime.HasValue && attendance.CheckOutTime.HasValue)
        {
            if (attendance.CheckOutTime.Value < attendance.CheckInTime.Value)
                return Error.Validation("AttendanceCorrection.InvalidRange", "Check-out time cannot be earlier than check-in time.");

            attendance.WorkedHours = attendance.CheckOutTime.Value - attendance.CheckInTime.Value;
        }

        attendance.StatusId = AttendanceStatusIds.Present;
        attendance.MarkAsModified(CurrentUser.Id ?? Guid.Empty);

        return null;
    }

    private static bool TryExtractDescriptionValue(string description, string key, out string value)
    {
        value = string.Empty;
        var prefix = key + ":";
        var lines = description.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (!line.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                continue;

            value = line.Substring(prefix.Length).Trim();
            return !string.IsNullOrWhiteSpace(value);
        }

        return false;
    }

    /// <summary>
    /// Creates a Loan record when HR approves a Personal request whose PersonalType name
    /// contains "Loan". Only creates the loan if HR provides the loan details.
    /// </summary>
    private async Task<Error?> TryCreateLoanAsync(
        EmployeeRequest employeeRequest,
        ApproveRequestCommand request,
        CancellationToken cancellationToken)
    {
        var personalTypeName = employeeRequest.PersonalDetail?.PersonalType?.NameEn ?? "";
        if (!personalTypeName.Contains("Loan", StringComparison.OrdinalIgnoreCase))
            return null; // Not a loan request, skip

        // Loan details are optional — HR may approve without creating a loan
        if (!request.LoanAmount.HasValue || !request.LoanInstallmentMonths.HasValue)
            return null;

        var loanAmount = request.LoanAmount.Value;
        var installmentMonths = request.LoanInstallmentMonths.Value;
        var monthlyDeduction = request.LoanMonthlyDeduction
            ?? Math.Round(loanAmount / installmentMonths, 2);
        var startDate = request.LoanStartDate ?? DateTime.UtcNow;

        if (loanAmount <= 0)
            return Error.Validation(description: "Loan amount must be greater than zero.");
        if (installmentMonths <= 0)
            return Error.Validation(description: "Installment months must be greater than zero.");

        var loan = new Loan
        {
            EmployeeId = employeeRequest.EmployeeId,
            LoanName = employeeRequest.Title,
            TotalAmount = loanAmount,
            RemainingAmount = loanAmount,
            MonthlyDeduction = monthlyDeduction,
            InstallmentMonths = installmentMonths,
            StartDate = startDate,
            EndDate = startDate.AddMonths(installmentMonths),
            IsActive = true,
            Notes = $"Auto-created from request: {employeeRequest.Title}",
            TenantId = employeeRequest.TenantId,
            BranchId = employeeRequest.BranchId
        };

        await _context.Loans.AddAsync(loan, cancellationToken);
        return null;
    }
}
