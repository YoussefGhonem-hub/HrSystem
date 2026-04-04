using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.GetPendingApprovalRequests;

/// <summary>
/// Returns a paginated list of requests waiting for the current user's approval across ALL request types.
/// - DepartmentManager: sees Pending requests from their direct reports.
/// - HR (HRManager/HRSpecialist/OrganizationAdmin): sees ManagerApproved requests for their branch.
/// </summary>
public record GetPendingApprovalRequestsQuery(
    string? RequestTypeCode = null,
    DateTime? StartDateFrom = null,
    DateTime? StartDateTo = null,
    string? SortBy = null,
    bool SortDescending = false,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<ErrorOr<GenericResponse<PagedResult<EmployeeRequestDto>>>>;

public class GetPendingApprovalRequestsQueryHandler
    : IRequestHandler<GetPendingApprovalRequestsQuery, ErrorOr<GenericResponse<PagedResult<EmployeeRequestDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetPendingApprovalRequestsQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<PagedResult<EmployeeRequestDto>>>> Handle(
        GetPendingApprovalRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var roles = CurrentUser.Roles;
        var employeeId = CurrentUser.EmployeeId;
        var branchId = CurrentUser.BranchId;

        var isHR = roles.Any(r =>
            r == RoleNames.HRManager ||
            r == RoleNames.HRSpecialist ||
            r == RoleNames.OrganizationAdmin);

        var isManager = roles.Any(r => r == RoleNames.DepartmentManager);

        if (!isHR && !isManager)
            return Error.Forbidden(description: "You do not have permission to approve requests.");

        var query = _context.EmployeeRequests
            .AsNoTracking()
            .Include(r => r.RequestTypeRef)
            .Include(r => r.Employee)
            .Include(r => r.VacationDetail).ThenInclude(v => v!.VacationType)
            .Include(r => r.PermissionDetail).ThenInclude(p => p!.PermissionType)
            .Include(r => r.TrainingDetail).ThenInclude(t => t!.TrainingType)
            .Include(r => r.OvertimeDetail).ThenInclude(o => o!.OvertimeType)
            .Include(r => r.MiscellaneousDetail).ThenInclude(m => m!.MiscellaneousType)
            .Include(r => r.PersonalDetail).ThenInclude(p => p!.PersonalType)
            .Include(r => r.FeedbackDetail).ThenInclude(f => f!.FeedbackType)
            .AsQueryable();

        if (isHR)
        {
            // HR sees ManagerApproved requests for their branch
            query = query.Where(r => r.Status == EmployeeRequestStatus.ManagerApproved);

            if (branchId.HasValue)
                query = query.Where(r => r.BranchId == branchId);
        }
        else if (isManager && employeeId.HasValue)
        {
            // Manager sees Pending requests from their direct reports
            query = query.Where(r =>
                r.Status == EmployeeRequestStatus.Pending &&
                r.Employee.DirectManagerId == employeeId.Value);
        }
        else
        {
            return Error.Validation(description: "Unable to resolve your employee context.");
        }

        // Filters
        if (!string.IsNullOrEmpty(request.RequestTypeCode))
            query = query.Where(r => r.RequestTypeRef != null && r.RequestTypeRef.Code == request.RequestTypeCode);

        if (request.StartDateFrom.HasValue)
            query = query.Where(r => r.StartDate >= request.StartDateFrom.Value);

        if (request.StartDateTo.HasValue)
            query = query.Where(r => r.StartDate <= request.StartDateTo.Value);

        // Sorting
        query = request.SortBy?.ToLowerInvariant() switch
        {
            "status" => request.SortDescending
                ? query.OrderByDescending(r => r.Status)
                : query.OrderBy(r => r.Status),
            "employee" => request.SortDescending
                ? query.OrderByDescending(r => r.Employee.FirstNameEn)
                : query.OrderBy(r => r.Employee.FirstNameEn),
            "type" => request.SortDescending
                ? query.OrderByDescending(r => r.RequestTypeRef!.Code)
                : query.OrderBy(r => r.RequestTypeRef!.Code),
            _ => query.OrderByDescending(r => r.RequestedDate)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var entities = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = entities.Select(r => new EmployeeRequestDto
        {
            Id = r.Id,
            RequestTypeId = r.RequestTypeId,
            RequestTypeName = r.RequestTypeRef?.Code ?? "",
            Status = r.Status,
            EmployeeId = r.EmployeeId,
            EmployeeName = $"{r.Employee.FirstNameEn} {r.Employee.LastNameEn}",
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
            PendingAt = r.Status == EmployeeRequestStatus.Pending
                ? (r.Employee.DirectManagerId != null ? "Manager" : "HR")
                : r.Status == EmployeeRequestStatus.ManagerApproved
                    ? "HR"
                    : null,
            VacationDetail = r.VacationDetail != null ? new VacationDetailDto
            {
                VacationTypeId = r.VacationDetail.VacationTypeId,
                VacationTypeName = r.VacationDetail.VacationType?.NameEn,
                TotalDays = r.VacationDetail.TotalDays,
                ManagerId = r.VacationDetail.ManagerId,
                ManagerApprovalDate = r.VacationDetail.ManagerApprovalDate,
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
        }).ToList();

        var pagedResult = PagedResult<EmployeeRequestDto>.Create(dtos, totalCount, request.PageNumber, request.PageSize);
        return GenericResponse<PagedResult<EmployeeRequestDto>>.SuccessResult(pagedResult, $"Found {totalCount} pending approval requests.");
    }
}
