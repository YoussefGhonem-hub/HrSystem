using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

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

    public GetRequestDetailQueryHandler(ApplicationDbContext context) => _context = context;

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
            RequestTypeName = r.RequestTypeRef?.Code ?? "",
            Status = r.Status,
            EmployeeId = r.EmployeeId,
            EmployeeName = r.Employee?.FullNameEn,
            BranchId = r.Employee?.BranchId,
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
}
