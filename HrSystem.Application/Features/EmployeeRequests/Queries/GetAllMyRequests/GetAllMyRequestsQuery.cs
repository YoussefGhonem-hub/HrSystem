using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.GetAllMyRequests;

/// <summary>
/// Returns a paginated list of ALL request types submitted by the logged-in employee,
/// with optional filters by request type code and status.
/// </summary>
public record GetAllMyRequestsQuery(
    Guid EmployeeId,
    string? RequestTypeCode = null,
    EmployeeRequestStatus? Status = null,
    DateTime? StartDateFrom = null,
    DateTime? StartDateTo = null,
    string? SortBy = null,
    bool SortDescending = false,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<ErrorOr<GenericResponse<PagedResult<EmployeeRequestDto>>>>;

public class GetAllMyRequestsQueryHandler
    : IRequestHandler<GetAllMyRequestsQuery, ErrorOr<GenericResponse<PagedResult<EmployeeRequestDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetAllMyRequestsQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<PagedResult<EmployeeRequestDto>>>> Handle(
        GetAllMyRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.EmployeeRequests
            .AsNoTracking()
            .Include(r => r.RequestTypeRef)
            .Include(r => r.VacationDetail).ThenInclude(v => v!.VacationType)
            .Include(r => r.PermissionDetail).ThenInclude(p => p!.PermissionType)
            .Include(r => r.TrainingDetail).ThenInclude(t => t!.TrainingType)
            .Include(r => r.OvertimeDetail).ThenInclude(o => o!.OvertimeType)
            .Include(r => r.MiscellaneousDetail).ThenInclude(m => m!.MiscellaneousType)
            .Include(r => r.PersonalDetail).ThenInclude(p => p!.PersonalType)
            .Include(r => r.FeedbackDetail).ThenInclude(f => f!.FeedbackType)
            .Where(r => r.EmployeeId == request.EmployeeId);

        // Filters
        if (!string.IsNullOrEmpty(request.RequestTypeCode))
            query = query.Where(r => r.RequestTypeRef != null && r.RequestTypeRef.Code == request.RequestTypeCode);

        if (request.Status.HasValue)
            query = query.Where(r => r.Status == request.Status.Value);

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
            "title" => request.SortDescending
                ? query.OrderByDescending(r => r.Title)
                : query.OrderBy(r => r.Title),
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

        var dtos = entities.Select(MapToDto).ToList();

        var result = PagedResult<EmployeeRequestDto>.Create(dtos, totalCount, request.PageNumber, request.PageSize);
        return GenericResponse<PagedResult<EmployeeRequestDto>>.SuccessResult(result, $"Found {totalCount} requests.");
    }

    private static EmployeeRequestDto MapToDto(Domain.Entities.Requests.EmployeeRequest r) => new()
    {
        Id = r.Id,
        RequestTypeId = r.RequestTypeId,
        RequestTypeName = r.RequestTypeRef?.Code ?? "",
        Status = r.Status,
        EmployeeId = r.EmployeeId,
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
    };
}
