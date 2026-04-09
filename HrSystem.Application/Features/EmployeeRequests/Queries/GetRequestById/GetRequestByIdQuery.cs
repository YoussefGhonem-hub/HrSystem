using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.GetRequestById;

/// <summary>
/// Returns full details for any request (Vacation, Permission, Training, Overtime,
/// Miscellaneous, Personal, Feedback) by its ID. Eagerly loads the correct detail
/// entity based on the request type, eliminating the need for per-type endpoints.
/// </summary>
public record GetRequestByIdQuery(Guid Id) : IRequest<ErrorOr<GenericResponse<EmployeeRequestDto>>>;

public class GetRequestByIdQueryHandler
    : IRequestHandler<GetRequestByIdQuery, ErrorOr<GenericResponse<EmployeeRequestDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetRequestByIdQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<EmployeeRequestDto>>> Handle(
        GetRequestByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await _context.EmployeeRequests
            .Include(r => r.RequestTypeRef)
            .Include(r => r.Employee).ThenInclude(e => e.Department)
            .Include(r => r.Employee).ThenInclude(e => e.JobTitle)
            .Include(r => r.Employee).ThenInclude(e => e.Branch)
            .Include(r => r.VacationDetail).ThenInclude(v => v!.VacationType)
            .Include(r => r.PermissionDetail).ThenInclude(p => p!.PermissionType)
            .Include(r => r.TrainingDetail).ThenInclude(t => t!.TrainingType)
            .Include(r => r.OvertimeDetail).ThenInclude(o => o!.OvertimeType)
            .Include(r => r.MiscellaneousDetail).ThenInclude(m => m!.MiscellaneousType)
            .Include(r => r.PersonalDetail).ThenInclude(p => p!.PersonalType)
            .Include(r => r.FeedbackDetail).ThenInclude(f => f!.FeedbackType)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (entity == null)
            return Error.NotFound(description: "Request not found.");

        // Calculate overtime amount & payslip inclusion if this is an overtime request
        decimal? estimatedOvertimeAmount = null;
        bool isIncludedInPayslip = false;
        string? payslipPeriod = null;

        if (entity.OvertimeDetail != null)
        {
            var currentSalary = await _context.Salaries
                .AsNoTracking()
                .Where(s => s.EmployeeId == entity.EmployeeId && s.IsCurrent && !s.IsDeleted)
                .OrderByDescending(s => s.EffectiveDate)
                .FirstOrDefaultAsync(cancellationToken);

            if (currentSalary != null)
            {
                decimal hourlyRate = currentSalary.BasicSalary / 240m;
                var hours = entity.OvertimeDetail.ActualHours ?? entity.OvertimeDetail.PlannedHours;
                estimatedOvertimeAmount = Math.Round((decimal)hours.TotalHours * hourlyRate * entity.OvertimeDetail.Multiplier, 2);
            }

            var overtimeMonth = entity.OvertimeDetail.OvertimeDate.Month;
            var overtimeYear = entity.OvertimeDetail.OvertimeDate.Year;

            var payslip = await _context.Payslips
                .AsNoTracking()
                .Include(p => p.PayrollCycle)
                .Where(p => p.EmployeeId == entity.EmployeeId
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
            Id = entity.Id,
            RequestTypeId = entity.RequestTypeId,
            RequestTypeName = entity.RequestTypeRef?.Code ?? "",
            Status = entity.Status,
            EmployeeId = entity.EmployeeId,
            EmployeeName = entity.Employee?.FullNameEn,
            BranchId = entity.BranchId,
            Title = entity.Title,
            Description = entity.Description,
            RequestedDate = entity.RequestedDate,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            AttachmentUrl = entity.AttachmentUrl,
            ManagerComments = entity.ManagerComments,
            RejectionReason = entity.RejectionReason,
            ApprovedBy = entity.ApprovedBy,
            ApprovedDate = entity.ApprovedDate,
            ProcessedBy = entity.ProcessedBy,
            ProcessedDate = entity.ProcessedDate,
            VacationDetail = entity.VacationDetail != null ? new VacationDetailDto
            {
                VacationTypeId = entity.VacationDetail.VacationTypeId,
                VacationTypeName = entity.VacationDetail.VacationType?.NameEn,
                TotalDays = entity.VacationDetail.TotalDays,
                ManagerId = entity.VacationDetail.ManagerId,
                ManagerApprovalDate = entity.VacationDetail.ManagerApprovalDate,
                ManagerComments = entity.VacationDetail.ManagerComments,
                HRApprovedBy = entity.VacationDetail.HRApprovedBy,
                HRApprovalDate = entity.VacationDetail.HRApprovalDate,
                HRComments = entity.VacationDetail.HRComments,
                EmergencyContactName = entity.VacationDetail.EmergencyContactName,
                EmergencyContactPhone = entity.VacationDetail.EmergencyContactPhone
            } : null,
            PermissionDetail = entity.PermissionDetail != null ? new PermissionDetailDto
            {
                PermissionTypeId = entity.PermissionDetail.PermissionTypeId,
                PermissionTypeName = entity.PermissionDetail.PermissionType?.NameEn,
                PermissionDate = entity.PermissionDetail.PermissionDate,
                FromTime = entity.PermissionDetail.FromTime,
                ToTime = entity.PermissionDetail.ToTime,
                TotalHours = entity.PermissionDetail.TotalHours,
                Reason = entity.PermissionDetail.Reason,
                ManagerId = entity.PermissionDetail.ManagerId,
                ManagerApprovalDate = entity.PermissionDetail.ManagerApprovalDate,
                ManagerComments = entity.PermissionDetail.ManagerComments,
                LeaveDeduction = entity.PermissionDetail.LeaveDeduction
            } : null,
            TrainingDetail = entity.TrainingDetail != null ? new TrainingDetailDto
            {
                TrainingTypeId = entity.TrainingDetail.TrainingTypeId,
                TrainingTypeName = entity.TrainingDetail.TrainingType?.NameEn,
                TrainingName = entity.TrainingDetail.TrainingName,
                TrainingProvider = entity.TrainingDetail.TrainingProvider,
                TrainingLocation = entity.TrainingDetail.TrainingLocation,
                TrainingStartDate = entity.TrainingDetail.TrainingStartDate,
                TrainingEndDate = entity.TrainingDetail.TrainingEndDate,
                DurationDays = entity.TrainingDetail.DurationDays,
                EstimatedCost = entity.TrainingDetail.EstimatedCost,
                ApprovedBudget = entity.TrainingDetail.ApprovedBudget,
                Currency = entity.TrainingDetail.Currency,
                Objectives = entity.TrainingDetail.Objectives,
                ExpectedOutcome = entity.TrainingDetail.ExpectedOutcome,
                CertificationObtained = entity.TrainingDetail.CertificationObtained,
                CertificateUrl = entity.TrainingDetail.CertificateUrl
            } : null,
            OvertimeDetail = entity.OvertimeDetail != null ? new OvertimeDetailDto
            {
                OvertimeTypeId = entity.OvertimeDetail.OvertimeTypeId,
                OvertimeTypeName = entity.OvertimeDetail.OvertimeType?.NameEn,
                OvertimeDate = entity.OvertimeDetail.OvertimeDate,
                PlannedHours = entity.OvertimeDetail.PlannedHours,
                ActualHours = entity.OvertimeDetail.ActualHours,
                Multiplier = entity.OvertimeDetail.Multiplier,
                ProjectCode = entity.OvertimeDetail.ProjectCode,
                TaskDescription = entity.OvertimeDetail.TaskDescription,
                ApprovedBy = entity.OvertimeDetail.ApprovedBy,
                ApprovedDate = entity.OvertimeDetail.ApprovedDate,
                ApprovalNotes = entity.OvertimeDetail.ApprovalNotes,
                EstimatedOvertimeAmount = estimatedOvertimeAmount,
                IsIncludedInPayslip = isIncludedInPayslip,
                PayslipPeriod = payslipPeriod
            } : null,
            MiscellaneousDetail = entity.MiscellaneousDetail != null ? new MiscellaneousDetailDto
            {
                MiscellaneousTypeId = entity.MiscellaneousDetail.MiscellaneousTypeId,
                MiscellaneousTypeName = entity.MiscellaneousDetail.MiscellaneousType?.NameEn,
                AdditionalNotes = entity.MiscellaneousDetail.AdditionalNotes,
                ReferenceNumber = entity.MiscellaneousDetail.ReferenceNumber,
                Priority = entity.MiscellaneousDetail.Priority,
                ExpectedCompletionDate = entity.MiscellaneousDetail.ExpectedCompletionDate
            } : null,
            PersonalDetail = entity.PersonalDetail != null ? new PersonalDetailDto
            {
                PersonalTypeId = entity.PersonalDetail.PersonalTypeId,
                PersonalTypeName = entity.PersonalDetail.PersonalType?.NameEn,
                Reason = entity.PersonalDetail.Reason,
                IsUrgent = entity.PersonalDetail.IsUrgent,
                RequiresConfidentiality = entity.PersonalDetail.RequiresConfidentiality
            } : null,
            FeedbackDetail = entity.FeedbackDetail != null ? new FeedbackDetailDto
            {
                FeedbackTypeId = entity.FeedbackDetail.FeedbackTypeId,
                FeedbackTypeName = entity.FeedbackDetail.FeedbackType?.NameEn,
                FeedbackContent = entity.FeedbackDetail.FeedbackContent,
                IsAnonymous = entity.FeedbackDetail.IsAnonymous,
                Rating = entity.FeedbackDetail.Rating,
                TargetDepartment = entity.FeedbackDetail.TargetDepartment,
                TargetPerson = entity.FeedbackDetail.TargetPerson,
                SuggestedImprovement = entity.FeedbackDetail.SuggestedImprovement,
                ResponseRequired = entity.FeedbackDetail.ResponseRequired,
                ResponseContent = entity.FeedbackDetail.ResponseContent,
                ResponseDate = entity.FeedbackDetail.ResponseDate
            } : null
        };

        return new GenericResponse<EmployeeRequestDto>
        {
            Success = true,
            Message = "Request details retrieved successfully.",
            Data = dto
        };
    }
}
