using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.GetRequestsDashboard;

public record GetRequestsOverviewQuery(
    string? SearchTerm = null,
    string? RequestTypeCode = null,
    EmployeeRequestStatus? Status = null,
    DateTime? RequestedFrom = null,
    DateTime? RequestedTo = null,
    string? SortBy = null,
    bool SortDescending = true,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<ErrorOr<GenericResponse<RequestsOverviewDto>>>;

public record RequestsOverviewDto
{
    public RequestsOverviewStatsDto Stats { get; init; } = new();
    public PagedResult<RequestListItemDto> Requests { get; init; } = null!;
}

public record RequestsOverviewStatsDto
{
    public int PendingRequests { get; init; }
    public int ApprovedToday { get; init; }
    public int RejectedRequests { get; init; }
    public int MonthlyTotalRequests { get; init; }
}

public record RequestListItemDto
{
    public Guid Id { get; init; }
    public string RequestTitle { get; init; } = string.Empty;
    public string RequestTypeCode { get; init; } = string.Empty;
    public string RequestTypeName { get; init; } = string.Empty;
    public EmployeeRequestStatus Status { get; init; }
    public DateTime RequestedDate { get; init; }
    public Guid EmployeeId { get; init; }
    public string EmployeeCode { get; init; } = string.Empty;
    public string EmployeeName { get; init; } = string.Empty;
    public string? EmployeeProfilePictureUrl { get; init; }
    public Guid? AttendanceCorrectionTypeId { get; init; }
    public string? AttendanceCorrectionTypeName { get; init; }

    /// <summary>
    /// Indicates which department currently holds the request for action.
    /// "Manager" → awaiting direct manager approval.
    /// "HR Department" → awaiting HR approval.
    /// Null → request is no longer pending (approved/rejected/cancelled).
    /// </summary>
    public string? PendingAt { get; init; }
}

public class GetRequestsOverviewQueryHandler
    : IRequestHandler<GetRequestsOverviewQuery, ErrorOr<GenericResponse<RequestsOverviewDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetRequestsOverviewQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<RequestsOverviewDto>>> Handle(
        GetRequestsOverviewQuery request,
        CancellationToken cancellationToken)
    {
        var baseQuery = _context.EmployeeRequests
            .AsNoTracking()
            .Include(r => r.Employee)
                .ThenInclude(e => e.DirectManager)
            .Include(r => r.RequestTypeRef)
            .Where(r => !r.IsDeleted);

        var listQuery = ApplyFilters(baseQuery, request, includeStatusFilter: true);
        var statsQuery = ApplyFilters(baseQuery, request, includeStatusFilter: false);

        var today = DateTime.UtcNow.Date;
        var monthStart = new DateTime(today.Year, today.Month, 1);

        var statsSeed = await statsQuery
            .Select(r => new { r.Status, r.ApprovedDate, r.RequestedDate })
            .ToListAsync(cancellationToken);

        var pendingRequests = statsSeed.Count(r =>
            r.Status == EmployeeRequestStatus.Pending ||
            r.Status == EmployeeRequestStatus.ManagerApproved);

        var approvedToday = statsSeed.Count(r =>
            r.Status == EmployeeRequestStatus.Approved &&
            r.ApprovedDate.HasValue &&
            r.ApprovedDate.Value.Date == today);

        var rejectedRequests = statsSeed.Count(r => r.Status == EmployeeRequestStatus.Rejected);

        var monthlyTotal = statsSeed.Count(r => r.RequestedDate.Date >= monthStart);

        var correctionTypesById = await _context.AttendanceCorrectionTypes
            .AsNoTracking()
            .Where(t => t.IsActive && !t.IsDeleted)
            .Select(t => new { t.Id, t.NameEn })
            .ToDictionaryAsync(t => t.Id, t => t.NameEn, cancellationToken);

        var sortedQuery = ApplySorting(listQuery, request.SortBy, request.SortDescending);
        var totalCount = await sortedQuery.CountAsync(cancellationToken);

        var pageItems = await sortedQuery
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var requestItems = pageItems.Select(r =>
        {
            var correctionTypeId = ParseAttendanceCorrectionTypeId(r.Description);
            var correctionTypeName = correctionTypeId.HasValue && correctionTypesById.TryGetValue(correctionTypeId.Value, out var name)
                ? name
                : null;

            return new RequestListItemDto
            {
                Id = r.Id,
                RequestTitle = string.IsNullOrWhiteSpace(r.Title)
                    ? (r.RequestTypeRef != null ? r.RequestTypeRef.Code : string.Empty)
                    : r.Title,
                RequestTypeCode = r.RequestTypeRef != null ? r.RequestTypeRef.Code : string.Empty,
                RequestTypeName = r.RequestTypeRef != null ? r.RequestTypeRef.NameEn : string.Empty,
                Status = r.Status,
                RequestedDate = r.RequestedDate,
                EmployeeId = r.EmployeeId,
                EmployeeCode = r.Employee.EmployeeCode,
                EmployeeName = r.Employee != null
                    ? string.Concat(r.Employee.FirstNameEn, " ", r.Employee.LastNameEn)
                    : string.Empty,
                EmployeeProfilePictureUrl = r.Employee != null ? r.Employee.ProfilePictureUrl : null,
                AttendanceCorrectionTypeId = correctionTypeId,
                AttendanceCorrectionTypeName = correctionTypeName,
                PendingAt = r.Status == EmployeeRequestStatus.Pending
                    ? (r.Employee?.DirectManager != null
                        ? r.Employee.DirectManager.FullNameEn
                        : "HR Department")
                    : r.Status == EmployeeRequestStatus.ManagerApproved
                        ? "HR Department"
                        : null
            };
        }).ToList();

        var pagedRequests = PagedResult<RequestListItemDto>.Create(
            requestItems,
            totalCount,
            request.PageNumber,
            request.PageSize);

        var dto = new RequestsOverviewDto
        {
            Stats = new RequestsOverviewStatsDto
            {
                PendingRequests = pendingRequests,
                ApprovedToday = approvedToday,
                RejectedRequests = rejectedRequests,
                MonthlyTotalRequests = monthlyTotal
            },
            Requests = pagedRequests
        };

        return GenericResponse<RequestsOverviewDto>.SuccessResult(dto, "Requests overview loaded successfully.");
    }

    private static IQueryable<Domain.Entities.Requests.EmployeeRequest> ApplyFilters(
        IQueryable<Domain.Entities.Requests.EmployeeRequest> query,
        GetRequestsOverviewQuery request,
        bool includeStatusFilter)
    {
        if (!string.IsNullOrWhiteSpace(request.RequestTypeCode))
        {
            query = query.Where(r => r.RequestTypeRef != null && r.RequestTypeRef.Code == request.RequestTypeCode);
        }

        if (includeStatusFilter && request.Status.HasValue)
        {
            query = query.Where(r => r.Status == request.Status.Value);
        }

        if (request.RequestedFrom.HasValue)
        {
            var fromDate = request.RequestedFrom.Value.Date;
            query = query.Where(r => r.RequestedDate.Date >= fromDate);
        }

        if (request.RequestedTo.HasValue)
        {
            var toDate = request.RequestedTo.Value.Date;
            query = query.Where(r => r.RequestedDate.Date <= toDate);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = $"%{request.SearchTerm.Trim()}%";
            query = query.Where(r =>
                EF.Functions.Like(r.Employee.EmployeeCode, term) ||
                EF.Functions.Like(r.Employee.FirstNameEn + " " + r.Employee.LastNameEn, term) ||
                EF.Functions.Like(r.Employee.FirstNameAr + " " + r.Employee.LastNameAr, term) ||
                EF.Functions.Like(r.Title, term));
        }

        return query;
    }

    private static IQueryable<Domain.Entities.Requests.EmployeeRequest> ApplySorting(
        IQueryable<Domain.Entities.Requests.EmployeeRequest> query,
        string? sortBy,
        bool sortDescending)
    {
        return sortBy?.ToLowerInvariant() switch
        {
            "employee" => sortDescending
                ? query.OrderByDescending(r => r.Employee.FirstNameEn).ThenByDescending(r => r.Employee.LastNameEn)
                : query.OrderBy(r => r.Employee.FirstNameEn).ThenBy(r => r.Employee.LastNameEn),
            "type" => sortDescending
                ? query.OrderByDescending(r => r.RequestTypeRef!.NameEn)
                : query.OrderBy(r => r.RequestTypeRef!.NameEn),
            "status" => sortDescending
                ? query.OrderByDescending(r => r.Status)
                : query.OrderBy(r => r.Status),
            _ => sortDescending
                ? query.OrderByDescending(r => r.RequestedDate)
                : query.OrderBy(r => r.RequestedDate)
        };
    }

    private static Guid? ParseAttendanceCorrectionTypeId(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return null;
        }

        if (!TryExtractDescriptionValue(description, "AttendanceCorrectionTypeId", out var rawId) ||
            !Guid.TryParse(rawId, out var correctionTypeId))
        {
            return null;
        }

        return correctionTypeId;
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
}
