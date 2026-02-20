using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.LeaveBalances.Queries.GetLeaveReport;

public class GetLeaveReportQueryHandler
    : IRequestHandler<GetLeaveReportQuery, ErrorOr<GenericResponse<LeaveReportResponseDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetLeaveReportQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<LeaveReportResponseDto>>> Handle(
        GetLeaveReportQuery request,
        CancellationToken cancellationToken)
    {
        // ── Base query: only Vacation-type requests that have a VacationDetail ──
        var baseQuery = _context.EmployeeRequests
            .Include(r => r.RequestTypeRef)
            .Include(r => r.Employee)
                .ThenInclude(e => e.Department)
            .Include(r => r.VacationDetail)
                .ThenInclude(v => v!.VacationType)
            .Where(r => r.RequestTypeRef != null && r.RequestTypeRef.Code == "Vacation")
            .Where(r => r.VacationDetail != null)
            .AsQueryable();

        // ── Apply shared filters (used by both statistics & grid) ──
        baseQuery = ApplyFilters(baseQuery, request);

        // ── 1. Statistics ──
        var statistics = await CalculateStatistics(baseQuery, cancellationToken);

        // ── 2. Sorting ──
        var sortedQuery = ApplySorting(baseQuery, request.SortBy, request.SortDescending);

        // ── 3. Total count (before pagination) ──
        var totalCount = await sortedQuery.CountAsync(cancellationToken);

        // ── 4. Paginated grid ──
        var items = await sortedQuery
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(r => new LeaveReportListDto
            {
                RequestId = r.Id,
                EmployeeCode = r.Employee.EmployeeCode,
                EmployeeNameEn = r.Employee.FullNameEn,
                EmployeeNameAr = r.Employee.FullNameAr,
                DepartmentName = r.Employee.Department != null
                    ? r.Employee.Department.NameEn
                    : "No Department",
                VacationTypeNameEn = r.VacationDetail!.VacationType != null
                    ? r.VacationDetail.VacationType.NameEn
                    : string.Empty,
                VacationTypeNameAr = r.VacationDetail.VacationType != null
                    ? r.VacationDetail.VacationType.NameAr
                    : string.Empty,
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                TotalDays = r.VacationDetail.TotalDays,
                Status = r.Status.ToString(),
                RequestedDate = r.RequestedDate
            })
            .ToListAsync(cancellationToken);

        var pagedResult = PagedResult<LeaveReportListDto>.Create(
            items,
            totalCount,
            request.PageNumber,
            request.PageSize);

        var response = new LeaveReportResponseDto
        {
            Statistics = statistics,
            LeaveRequests = pagedResult
        };

        return GenericResponse<LeaveReportResponseDto>.SuccessResult(
            response,
            "Leave report retrieved successfully");
    }

    // ─────────────────────────────────────────────────────────────
    //  Filters
    // ─────────────────────────────────────────────────────────────
    private static IQueryable<Domain.Entities.Requests.EmployeeRequest> ApplyFilters(
        IQueryable<Domain.Entities.Requests.EmployeeRequest> query,
        GetLeaveReportQuery request)
    {
        if (request.Year.HasValue)
        {
            query = query.Where(r =>
                (r.StartDate.HasValue && r.StartDate.Value.Year == request.Year.Value) ||
                (r.EndDate.HasValue && r.EndDate.Value.Year == request.Year.Value));
        }

        if (request.EmployeeId.HasValue)
        {
            query = query.Where(r => r.EmployeeId == request.EmployeeId.Value);
        }

        if (request.DepartmentId.HasValue)
        {
            query = query.Where(r => r.Employee.DepartmentId == request.DepartmentId.Value);
        }

        if (request.VacationTypeId.HasValue)
        {
            query = query.Where(r =>
                r.VacationDetail != null &&
                r.VacationDetail.VacationTypeId == request.VacationTypeId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(r => r.Status == request.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(r =>
                r.Employee.EmployeeCode.ToLower().Contains(term) ||
                r.Employee.FirstNameEn.ToLower().Contains(term) ||
                r.Employee.LastNameEn.ToLower().Contains(term) ||
                r.Employee.FirstNameAr.Contains(term) ||
                r.Employee.LastNameAr.Contains(term));
        }

        return query;
    }

    // ─────────────────────────────────────────────────────────────
    //  Statistics
    // ─────────────────────────────────────────────────────────────
    private static async Task<LeaveReportStatisticsDto> CalculateStatistics(
        IQueryable<Domain.Entities.Requests.EmployeeRequest> query,
        CancellationToken cancellationToken)
    {
        // Materialize a lightweight projection so we can compute in-memory
        var rows = await query
            .Select(r => new
            {
                r.Status,
                TotalDays = r.VacationDetail!.TotalDays,
                VacationTypeId = r.VacationDetail.VacationTypeId,
                VacationTypeNameEn = r.VacationDetail.VacationType != null
                    ? r.VacationDetail.VacationType.NameEn : string.Empty,
                VacationTypeNameAr = r.VacationDetail.VacationType != null
                    ? r.VacationDetail.VacationType.NameAr : string.Empty
            })
            .ToListAsync(cancellationToken);

        var total = rows.Count;
        var approved = rows.Count(r =>
            r.Status == EmployeeRequestStatus.Approved ||
            r.Status == EmployeeRequestStatus.Completed);
        var pending = rows.Count(r =>
            r.Status == EmployeeRequestStatus.Pending ||
            r.Status == EmployeeRequestStatus.ManagerApproved);
        var rejected = rows.Count(r => r.Status == EmployeeRequestStatus.Rejected);
        var totalDays = rows.Sum(r => r.TotalDays);
        var avgDays = total > 0 ? Math.Round(totalDays / total, 2) : 0;

        var breakdown = rows
            .GroupBy(r => r.VacationTypeId)
            .Select(g => new VacationTypeBreakdownDto
            {
                VacationTypeId = g.Key,
                VacationTypeNameEn = g.First().VacationTypeNameEn,
                VacationTypeNameAr = g.First().VacationTypeNameAr,
                RequestCount = g.Count(),
                TotalDays = g.Sum(x => x.TotalDays)
            })
            .OrderByDescending(b => b.TotalDays)
            .ToList();

        return new LeaveReportStatisticsDto
        {
            TotalLeaveRequests = total,
            ApprovedRequests = approved,
            PendingRequests = pending,
            RejectedRequests = rejected,
            TotalDaysUsed = totalDays,
            AverageDaysPerRequest = avgDays,
            BreakdownByVacationType = breakdown
        };
    }

    // ─────────────────────────────────────────────────────────────
    //  Sorting
    // ─────────────────────────────────────────────────────────────
    private static IQueryable<Domain.Entities.Requests.EmployeeRequest> ApplySorting(
        IQueryable<Domain.Entities.Requests.EmployeeRequest> query,
        string? sortBy,
        bool descending)
    {
        return (sortBy?.ToLower()) switch
        {
            "employeecode" => descending
                ? query.OrderByDescending(r => r.Employee.EmployeeCode)
                : query.OrderBy(r => r.Employee.EmployeeCode),
            "employeename" => descending
                ? query.OrderByDescending(r => r.Employee.FirstNameEn)
                : query.OrderBy(r => r.Employee.FirstNameEn),
            "department" => descending
                ? query.OrderByDescending(r => r.Employee.Department.NameEn)
                : query.OrderBy(r => r.Employee.Department.NameEn),
            "vacationtype" => descending
                ? query.OrderByDescending(r => r.VacationDetail!.VacationType.NameEn)
                : query.OrderBy(r => r.VacationDetail!.VacationType.NameEn),
            "startdate" => descending
                ? query.OrderByDescending(r => r.StartDate)
                : query.OrderBy(r => r.StartDate),
            "enddate" => descending
                ? query.OrderByDescending(r => r.EndDate)
                : query.OrderBy(r => r.EndDate),
            "totaldays" => descending
                ? query.OrderByDescending(r => r.VacationDetail!.TotalDays)
                : query.OrderBy(r => r.VacationDetail!.TotalDays),
            "status" => descending
                ? query.OrderByDescending(r => r.Status)
                : query.OrderBy(r => r.Status),
            _ => descending
                ? query.OrderByDescending(r => r.RequestedDate)
                : query.OrderBy(r => r.RequestedDate)
        };
    }
}
