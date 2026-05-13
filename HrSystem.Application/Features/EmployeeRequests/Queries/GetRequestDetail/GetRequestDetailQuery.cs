using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Storage.AWS3.Services;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.GetRequestDetail;

/// <summary>
/// Unified query – returns full details for ANY request type by its ID.
/// Eager-loads every possible detail navigation so the caller never needs to
/// know the request type in advance.
/// </summary>
public record GetRequestDetailQuery(Guid Id) : IRequest<ErrorOr<GenericResponse<EmployeeRequestDto>>>;

public class GetRequestDetailQueryHandler
    : IRequestHandler<GetRequestDetailQuery, ErrorOr<GenericResponse<EmployeeRequestDto>>>
{
    private readonly ApplicationDbContext _context;
    private readonly IStorageService _storageService;

    public GetRequestDetailQueryHandler(ApplicationDbContext context, IStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    public async Task<ErrorOr<GenericResponse<EmployeeRequestDto>>> Handle(
        GetRequestDetailQuery request,
        CancellationToken cancellationToken)
    {
        var r = await _context.EmployeeRequests
            .AsNoTracking()
            .Include(e => e.RequestTypeRef)
            .Include(e => e.Employee).ThenInclude(emp => emp.Department)
            .Include(e => e.Employee).ThenInclude(emp => emp.JobTitle)
            .Include(e => e.Employee).ThenInclude(emp => emp.Branch)
            .Include(e => e.ApprovedByUser)
            .Include(e => e.ProcessedByUser)
            .Include(e => e.VacationDetail).ThenInclude(v => v!.VacationType)
            .Include(e => e.PermissionDetail).ThenInclude(p => p!.PermissionType)
            .Include(e => e.TrainingDetail).ThenInclude(t => t!.TrainingType)
            .Include(e => e.OvertimeDetail).ThenInclude(o => o!.OvertimeType)
            .Include(e => e.MiscellaneousDetail).ThenInclude(m => m!.MiscellaneousType)
            .Include(e => e.PersonalDetail).ThenInclude(p => p!.PersonalType)
            .Include(e => e.FeedbackDetail).ThenInclude(f => f!.FeedbackType)
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (r == null)
            return Error.NotFound("Request.NotFound", "Employee request not found.");

        // Calculate overtime amount & payslip inclusion if this is an overtime request
        decimal? estimatedOvertimeAmount = null;
        bool isIncludedInPayslip = false;
        string? payslipPeriod = null;

        if (r.OvertimeDetail != null)
        {
            // Get the employee's current salary for hourly-rate calculation
            var currentSalary = await _context.Salaries
                .AsNoTracking()
                .Where(s => s.EmployeeId == r.EmployeeId && s.IsCurrent && !s.IsDeleted)
                .OrderByDescending(s => s.EffectiveDate)
                .FirstOrDefaultAsync(cancellationToken);

            if (currentSalary != null)
            {
                decimal hourlyRate = currentSalary.BasicSalary / 240m;
                var hours = r.OvertimeDetail.ActualHours ?? r.OvertimeDetail.PlannedHours;
                estimatedOvertimeAmount = Math.Round((decimal)hours.TotalHours * hourlyRate * r.OvertimeDetail.Multiplier, 2);
            }

            // Check if a payslip exists for this employee in the overtime month
            var overtimeMonth = r.OvertimeDetail.OvertimeDate.Month;
            var overtimeYear = r.OvertimeDetail.OvertimeDate.Year;

            var payslip = await _context.Payslips
                .AsNoTracking()
                .Include(p => p.PayrollCycle)
                .Where(p => p.EmployeeId == r.EmployeeId
                    && p.PayrollCycle.Month == overtimeMonth
                    && p.PayrollCycle.Year == overtimeYear
                    && !p.IsDeleted
                    && p.OvertimeAmount > 0)
                .FirstOrDefaultAsync(cancellationToken);

            if (payslip != null)
            {
                isIncludedInPayslip = true;
                payslipPeriod = new DateTime(overtimeYear, overtimeMonth, 1).ToString("MMMM yyyy");
            }
        }

        var dto = new EmployeeRequestDto
        {
            Id = r.Id,
            RequestTypeId = r.RequestTypeId,
            RequestTypeCode = r.RequestTypeRef?.Code ?? "",
            RequestTypeName = r.RequestTypeRef?.NameEn ?? "",
            Status = r.Status,
            EmployeeId = r.EmployeeId,
            EmployeeName = r.Employee?.FullNameEn,
            EmployeeCode = r.Employee?.EmployeeCode,
            BranchId = r.Employee?.BranchId,
            Title = r.Title,
            Description = r.Description,
            RequestedDate = r.RequestedDate,
            StartDate = r.StartDate,
            EndDate = r.EndDate,
            AttachmentUrl = await ResolveAttachmentUrlAsync(r.AttachmentUrl, cancellationToken),
            ManagerComments = r.ManagerComments,
            RejectionReason = r.RejectionReason,
            ApprovedBy = r.ApprovedBy,
            ApprovedByName = r.ApprovedByUser != null
                ? (!string.IsNullOrWhiteSpace(r.ApprovedByUser.FullName)
                    ? r.ApprovedByUser.FullName
                    : r.ApprovedByUser.UserName)
                : null,
            ApprovedDate = r.ApprovedDate,
            ProcessedBy = r.ProcessedBy,
            ProcessedByName = r.ProcessedByUser != null
                ? (!string.IsNullOrWhiteSpace(r.ProcessedByUser.FullName)
                    ? r.ProcessedByUser.FullName
                    : r.ProcessedByUser.UserName)
                : null,
            ProcessedDate = r.ProcessedDate,
            AttendanceCorrectionDetail = await BuildAttendanceCorrectionDetailAsync(r.Description, cancellationToken),

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
                ApprovalNotes = r.OvertimeDetail.ApprovalNotes,
                EstimatedOvertimeAmount = estimatedOvertimeAmount,
                IsIncludedInPayslip = isIncludedInPayslip,
                PayslipPeriod = payslipPeriod
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
                RequiresConfidentiality = r.PersonalDetail.RequiresConfidentiality,
                PreferredContactMethod = r.PersonalDetail.PreferredContactMethod,
                AdditionalContactInfo = r.PersonalDetail.AdditionalContactInfo
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

        return GenericResponse<EmployeeRequestDto>.SuccessResult(dto, "Request details retrieved successfully.");
    }

    private async Task<AttendanceCorrectionDetailDto?> BuildAttendanceCorrectionDetailAsync(
        string? description,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return null;
        }

        if (!TryExtractDescriptionValue(description, "AttendanceCorrectionTypeId", out var correctionTypeIdRaw) ||
            !Guid.TryParse(correctionTypeIdRaw, out var correctionTypeId))
        {
            return null;
        }

        if (!TryExtractDescriptionValue(description, "AttendanceDate", out var attendanceDateRaw) ||
            !DateTime.TryParse(attendanceDateRaw, out var attendanceDate))
        {
            return null;
        }

        if (!TryExtractDescriptionValue(description, "CorrectedTime", out var correctedTimeRaw) ||
            !TimeSpan.TryParse(correctedTimeRaw, out var correctedTime))
        {
            return null;
        }

        var correctionTypeName = await _context.AttendanceCorrectionTypes
            .AsNoTracking()
            .Where(t => t.Id == correctionTypeId && t.IsActive && !t.IsDeleted)
            .Select(t => t.NameEn)
            .FirstOrDefaultAsync(cancellationToken);

        return new AttendanceCorrectionDetailDto
        {
            AttendanceCorrectionTypeId = correctionTypeId,
            AttendanceCorrectionTypeName = correctionTypeName,
            AttendanceDate = attendanceDate.Date,
            CorrectedTime = correctedTime
        };
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
            {
                continue;
            }

            value = line.Substring(prefix.Length).Trim();
            return !string.IsNullOrWhiteSpace(value);
        }

        return false;
    }

    private async Task<string?> ResolveAttachmentUrlAsync(string? storedValue, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(storedValue))
        {
            return null;
        }

        try
        {
            var url = await _storageService.DownloadFileUrl(storedValue, cancellationToken);
            return string.IsNullOrWhiteSpace(url) ? storedValue : url;
        }
        catch
        {
            // Return original value if presigning fails to avoid breaking the endpoint.
            return storedValue;
        }
    }
}
