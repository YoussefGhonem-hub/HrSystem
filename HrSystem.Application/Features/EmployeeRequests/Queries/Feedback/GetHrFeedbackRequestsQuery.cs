using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.Feedback;

public record GetHrFeedbackRequestsQuery(
    EmployeeRequestStatus? Status = null,
    Guid? FeedbackTypeId = null,
    DateTime? StartDateFrom = null,
    DateTime? StartDateTo = null,
    Guid? EmployeeId = null,
    string? SortBy = null,
    bool SortDescending = false,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<ErrorOr<GenericResponse<PagedResult<FeedbackRequestListDto>>>>;

public class GetHrFeedbackRequestsQueryHandler : IRequestHandler<GetHrFeedbackRequestsQuery, ErrorOr<GenericResponse<PagedResult<FeedbackRequestListDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetHrFeedbackRequestsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<PagedResult<FeedbackRequestListDto>>>> Handle(
        GetHrFeedbackRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var branchId = CurrentUser.BranchId;
        if (!branchId.HasValue)
            return Error.Unauthorized("User.NoBranch", "Current user is not associated with a branch");

        var query = _context.EmployeeRequests
            .Include(r => r.RequestTypeRef)
            .Include(r => r.Employee).ThenInclude(e => e.Department)
            .Include(r => r.Employee).ThenInclude(e => e.JobTitle)
            .Include(r => r.Employee).ThenInclude(e => e.Branch)
            .Include(r => r.FeedbackDetail).ThenInclude(f => f!.FeedbackType)
            .Where(r => r.RequestTypeRef != null && r.RequestTypeRef.Code == "Feedback")
            .Where(r => r.Employee.BranchId == branchId.Value)
            .AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(r => r.Status == request.Status.Value);

        if (request.FeedbackTypeId.HasValue)
            query = query.Where(r => r.FeedbackDetail != null && r.FeedbackDetail.FeedbackTypeId == request.FeedbackTypeId.Value);

        if (request.StartDateFrom.HasValue)
            query = query.Where(r => r.RequestedDate >= request.StartDateFrom.Value);

        if (request.StartDateTo.HasValue)
            query = query.Where(r => r.RequestedDate <= request.StartDateTo.Value);

        if (request.EmployeeId.HasValue)
            query = query.Where(r => r.EmployeeId == request.EmployeeId.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        query = ApplySorting(query, request.SortBy, request.SortDescending);

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(r => new FeedbackRequestListDto
            {
                Id = r.Id,
                EmployeeId = r.EmployeeId,
                EmployeeCode = r.Employee.EmployeeCode,
                EmployeeName = r.FeedbackDetail!.IsAnonymous ? "Anonymous" : r.Employee.FullNameEn,
                EmployeeNameAr = r.FeedbackDetail.IsAnonymous ? "مجهول" : r.Employee.FullNameAr,
                DepartmentName = r.FeedbackDetail.IsAnonymous ? null : r.Employee.Department.NameEn,
                JobTitle = r.FeedbackDetail.IsAnonymous ? null : r.Employee.JobTitle.TitleEn,
                BranchName = r.FeedbackDetail.IsAnonymous ? null : (r.Employee.Branch != null ? r.Employee.Branch.NameEn : null),
                FeedbackTypeId = r.FeedbackDetail.FeedbackTypeId,
                FeedbackTypeName = r.FeedbackDetail.FeedbackType != null ? r.FeedbackDetail.FeedbackType.NameEn : null,
                FeedbackTypeNameAr = r.FeedbackDetail.FeedbackType != null ? r.FeedbackDetail.FeedbackType.NameAr : null,
                IsAnonymous = r.FeedbackDetail.IsAnonymous,
                Rating = r.FeedbackDetail.Rating,
                TargetDepartment = r.FeedbackDetail.TargetDepartment,
                TargetPerson = r.FeedbackDetail.TargetPerson,
                ResponseRequired = r.FeedbackDetail.ResponseRequired,
                ResponseContent = r.FeedbackDetail.ResponseContent,
                ResponseDate = r.FeedbackDetail.ResponseDate,
                Title = r.Title,
                Description = r.Description,
                Status = r.Status,
                StatusName = r.Status.ToString(),
                RequestedDate = r.RequestedDate,
                ApprovedDate = r.ApprovedDate,
                RejectionReason = r.RejectionReason,
                CurrentApprovalLevel = r.Status == EmployeeRequestStatus.Pending ? 1 :
                                      r.Status == EmployeeRequestStatus.ManagerApproved ? 2 : 0
            })
            .ToListAsync(cancellationToken);

        var pagedResult = PagedResult<FeedbackRequestListDto>.Create(items, totalCount, request.PageNumber, request.PageSize);
        return GenericResponse<PagedResult<FeedbackRequestListDto>>.SuccessResult(pagedResult, "HR feedback requests retrieved successfully");
    }

    private static IQueryable<Domain.Entities.Requests.EmployeeRequest> ApplySorting(
        IQueryable<Domain.Entities.Requests.EmployeeRequest> query, string? sortBy, bool descending)
    {
        return sortBy?.ToLower() switch
        {
            "date" => descending
                ? query.OrderByDescending(r => r.RequestedDate)
                : query.OrderBy(r => r.RequestedDate),
            "status" => descending
                ? query.OrderByDescending(r => r.Status)
                : query.OrderBy(r => r.Status),
            "employee" => descending
                ? query.OrderByDescending(r => r.Employee.FullNameEn)
                : query.OrderBy(r => r.Employee.FullNameEn),
            _ => query.OrderByDescending(r => r.RequestedDate)
        };
    }
}
