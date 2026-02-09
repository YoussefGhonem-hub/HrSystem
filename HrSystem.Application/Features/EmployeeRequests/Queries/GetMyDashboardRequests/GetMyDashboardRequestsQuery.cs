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

namespace HrSystem.Application.Features.EmployeeRequests.Queries.GetMyDashboardRequests;

/// <summary>
/// A unified dashboard query that returns requests based on the logged-in user's role:
/// - Employee:          Only their own requests.
/// - DepartmentManager: Their own requests  +  Pending requests from direct reports that need manager approval.
/// - HRManager / HRSpecialist / OrganizationAdmin:
///                      Their own requests  +  ManagerApproved requests (in their branch) that need HR approval.
///
/// Approval flow:  Pending → ManagerApproved (manager) → Approved (HR).
/// A request is fully approved only when BOTH manager AND HR have approved it (Status == Approved).
/// </summary>
public record GetMyDashboardRequestsQuery(
    string? RequestTypeCode = null,
    EmployeeRequestStatus? Status = null,
    DateTime? StartDateFrom = null,
    DateTime? StartDateTo = null,
    string? SortBy = null,
    bool SortDescending = false,
    int MyRequestsPageNumber = 1,
    int MyRequestsPageSize = 20,
    int PendingApprovalPageNumber = 1,
    int PendingApprovalPageSize = 20
) : IRequest<ErrorOr<GenericResponse<MyDashboardRequestsDto>>>;

#region Response DTOs

public record MyDashboardRequestsDto
{
    /// <summary>The current user's role used to build this response.</summary>
    public string Role { get; init; } = string.Empty;

    /// <summary>The logged-in user's own requests (all roles).</summary>
    public PagedResult<EmployeeRequestDto> MyRequests { get; init; } = null!;

    /// <summary>
    /// Requests that need the current user's approval.
    /// - For Manager: Pending requests from direct reports.
    /// - For HR: ManagerApproved requests in the branch.
    /// - For Employee: always empty.
    /// </summary>
    public PagedResult<EmployeeRequestDto> PendingApprovalRequests { get; init; } = null!;

    /// <summary>Lightweight statistics for cards/counters.</summary>
    public DashboardStatsDto Stats { get; init; } = null!;
}

public record DashboardStatsDto
{
    public int MyRequestsTotal { get; init; }
    public int MyRequestsPending { get; init; }
    public int PendingApprovalTotal { get; init; }
    public int PendingApprovalPending { get; init; }
}

#endregion

public class GetMyDashboardRequestsQueryHandler
    : IRequestHandler<GetMyDashboardRequestsQuery, ErrorOr<GenericResponse<MyDashboardRequestsDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetMyDashboardRequestsQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<MyDashboardRequestsDto>>> Handle(
        GetMyDashboardRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var roles = CurrentUser.Roles;
        var employeeId = CurrentUser.EmployeeId;
        var branchId = CurrentUser.BranchId;

        if (!employeeId.HasValue)
            return Error.Validation(description: "The logged-in user is not linked to an employee profile.");

        // Determine the effective role (highest privilege wins)
        var isHR = roles.Any(r =>
            r == RoleNames.HRManager ||
            r == RoleNames.HRSpecialist ||
            r == RoleNames.OrganizationAdmin);

        var isManager = roles.Any(r => r == RoleNames.DepartmentManager);

        string effectiveRole = isHR
            ? "HR"
            : isManager
                ? "Manager"
                : "Employee";

        // ───────────────────────────────────────────────
        // 1) My own requests (all roles)
        // ───────────────────────────────────────────────
        var myQuery = ApplySorting(
            ApplyFilters(
                BaseQuery().Where(r => r.EmployeeId == employeeId.Value),
                request),
            request);

        var myTotalCount = await myQuery.CountAsync(cancellationToken);
        var myPendingCount = await myQuery
            .Where(r => r.Status == EmployeeRequestStatus.Pending)
            .CountAsync(cancellationToken);

        var myRequests = await PaginateAndProject(
            myQuery,
            request.MyRequestsPageNumber,
            request.MyRequestsPageSize,
            cancellationToken);

        // ───────────────────────────────────────────────
        // 2) Pending approval requests (role-dependent)
        // ───────────────────────────────────────────────
        PagedResult<EmployeeRequestDto> pendingApproval;
        int pendingApprovalTotalCount;

        if (isHR)
        {
            var pendingQuery = ApplySorting(
                ApplyFilters(
                    BaseQuery()
                        .Where(r =>
                            r.Status == EmployeeRequestStatus.ManagerApproved &&
                            r.EmployeeId != employeeId.Value &&
                            (!branchId.HasValue || r.BranchId == branchId)),
                    request),
                request);

            pendingApprovalTotalCount = await pendingQuery.CountAsync(cancellationToken);
            pendingApproval = await PaginateAndProject(
                pendingQuery,
                request.PendingApprovalPageNumber,
                request.PendingApprovalPageSize,
                cancellationToken);
        }
        else if (isManager)
        {
            var pendingQuery = ApplySorting(
                ApplyFilters(
                    BaseQuery()
                        .Where(r =>
                            r.Status == EmployeeRequestStatus.Pending &&
                            r.Employee.DirectManagerId == employeeId.Value &&
                            r.EmployeeId != employeeId.Value),
                    request),
                request);

            pendingApprovalTotalCount = await pendingQuery.CountAsync(cancellationToken);
            pendingApproval = await PaginateAndProject(
                pendingQuery,
                request.PendingApprovalPageNumber,
                request.PendingApprovalPageSize,
                cancellationToken);
        }
        else
        {
            pendingApprovalTotalCount = 0;
            pendingApproval = PagedResult<EmployeeRequestDto>.Create(
                new List<EmployeeRequestDto>(), 0,
                request.PendingApprovalPageNumber,
                request.PendingApprovalPageSize);
        }

        var stats = new DashboardStatsDto
        {
            MyRequestsTotal = myTotalCount,
            MyRequestsPending = myPendingCount,
            PendingApprovalTotal = pendingApprovalTotalCount,
            PendingApprovalPending = pendingApprovalTotalCount
        };

        var dto = new MyDashboardRequestsDto
        {
            Role = effectiveRole,
            MyRequests = myRequests,
            PendingApprovalRequests = pendingApproval,
            Stats = stats
        };

        return GenericResponse<MyDashboardRequestsDto>.SuccessResult(dto, "Dashboard loaded successfully.");
    }

    // ════════════════════════════════════════════════════════
    //  PRIVATE HELPERS
    // ════════════════════════════════════════════════════════

    /// <summary>Returns the logged-in employee's own requests.</summary>
    private async Task<PagedResult<EmployeeRequestDto>> BuildMyRequests(
        Guid employeeId,
        GetMyDashboardRequestsQuery request,
        CancellationToken ct)
    {
        var query = BaseQuery()
            .Where(r => r.EmployeeId == employeeId);

        query = ApplyFilters(query, request);
        query = ApplySorting(query, request);

        return await PaginateAndProject(query,
            request.MyRequestsPageNumber,
            request.MyRequestsPageSize,
            ct);
    }

    /// <summary>
    /// Manager sees Pending requests from their direct reports.
    /// Excludes the manager's own requests (they already appear in MyRequests).
    /// </summary>
    private async Task<PagedResult<EmployeeRequestDto>> BuildManagerPendingApproval(
        Guid managerEmployeeId,
        GetMyDashboardRequestsQuery request,
        CancellationToken ct)
    {
        var query = BaseQuery()
            .Where(r =>
                r.Status == EmployeeRequestStatus.Pending &&
                r.Employee.DirectManagerId == managerEmployeeId &&
                r.EmployeeId != managerEmployeeId);

        query = ApplyFilters(query, request);
        query = ApplySorting(query, request);

        return await PaginateAndProject(query,
            request.PendingApprovalPageNumber,
            request.PendingApprovalPageSize,
            ct);
    }

    /// <summary>
    /// HR sees ManagerApproved requests for their branch (ready for HR final approval).
    /// Excludes the HR user's own requests.
    /// </summary>
    private async Task<PagedResult<EmployeeRequestDto>> BuildHRPendingApproval(
        Guid? branchId,
        Guid hrEmployeeId,
        GetMyDashboardRequestsQuery request,
        CancellationToken ct)
    {
        var query = BaseQuery()
            .Where(r =>
                r.Status == EmployeeRequestStatus.ManagerApproved &&
                r.EmployeeId != hrEmployeeId);

        if (branchId.HasValue)
            query = query.Where(r => r.BranchId == branchId);

        query = ApplyFilters(query, request);
        query = ApplySorting(query, request);

        return await PaginateAndProject(query,
            request.PendingApprovalPageNumber,
            request.PendingApprovalPageSize,
            ct);
    }

    // ── Shared query builder ──

    private IQueryable<Domain.Entities.Requests.EmployeeRequest> BaseQuery()
    {
        return _context.EmployeeRequests
            .AsNoTracking()
            .Include(r => r.RequestTypeRef)
            .Include(r => r.Employee)
            .Include(r => r.VacationDetail).ThenInclude(v => v!.VacationType)
            .Include(r => r.PermissionDetail).ThenInclude(p => p!.PermissionType)
            .Include(r => r.TrainingDetail).ThenInclude(t => t!.TrainingType)
            .Include(r => r.OvertimeDetail).ThenInclude(o => o!.OvertimeType)
            .Include(r => r.MiscellaneousDetail).ThenInclude(m => m!.MiscellaneousType)
            .Include(r => r.PersonalDetail).ThenInclude(p => p!.PersonalType)
            .Include(r => r.FeedbackDetail).ThenInclude(f => f!.FeedbackType);
    }

    private static IQueryable<Domain.Entities.Requests.EmployeeRequest> ApplyFilters(
        IQueryable<Domain.Entities.Requests.EmployeeRequest> query,
        GetMyDashboardRequestsQuery request)
    {
        if (!string.IsNullOrEmpty(request.RequestTypeCode))
            query = query.Where(r => r.RequestTypeRef != null && r.RequestTypeRef.Code == request.RequestTypeCode);

        if (request.Status.HasValue)
            query = query.Where(r => r.Status == request.Status.Value);

        if (request.StartDateFrom.HasValue)
            query = query.Where(r => r.StartDate >= request.StartDateFrom.Value);

        if (request.StartDateTo.HasValue)
            query = query.Where(r => r.StartDate <= request.StartDateTo.Value);

        return query;
    }

    private static IQueryable<Domain.Entities.Requests.EmployeeRequest> ApplySorting(
        IQueryable<Domain.Entities.Requests.EmployeeRequest> query,
        GetMyDashboardRequestsQuery request)
    {
        return request.SortBy?.ToLowerInvariant() switch
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
    }

    private static async Task<PagedResult<EmployeeRequestDto>> PaginateAndProject(
        IQueryable<Domain.Entities.Requests.EmployeeRequest> query,
        int pageNumber,
        int pageSize,
        CancellationToken ct)
    {
        var totalCount = await query.CountAsync(ct);

        var entities = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var dtos = entities.Select(MapToDto).ToList();
        return PagedResult<EmployeeRequestDto>.Create(dtos, totalCount, pageNumber, pageSize);
    }

    private static EmployeeRequestDto MapToDto(Domain.Entities.Requests.EmployeeRequest r) => new()
    {
        Id = r.Id,
        RequestTypeId = r.RequestTypeId,
        RequestTypeName = r.RequestTypeRef?.Code ?? "",
        Status = r.Status,
        EmployeeId = r.EmployeeId,
        EmployeeName = r.Employee != null ? $"{r.Employee.FirstNameEn} {r.Employee.LastNameEn}" : null,
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
