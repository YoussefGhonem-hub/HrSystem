using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.Feedback;

public record GetManagerFeedbackOverviewQuery(
    int MyPageNumber = 1,
    int MyPageSize = 10,
    int PendingPageNumber = 1,
    int PendingPageSize = 10
) : IRequest<ErrorOr<GenericResponse<ManagerFeedbackOverviewDto>>>;

public class ManagerFeedbackOverviewDto
{
    public PagedResult<FeedbackRequestListDto> MyRequests { get; set; } = null!;
    public PagedResult<FeedbackRequestListDto> PendingApprovals { get; set; } = null!;
}

public class GetManagerFeedbackOverviewQueryHandler : IRequestHandler<GetManagerFeedbackOverviewQuery, ErrorOr<GenericResponse<ManagerFeedbackOverviewDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetManagerFeedbackOverviewQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<ManagerFeedbackOverviewDto>>> Handle(
        GetManagerFeedbackOverviewQuery request,
        CancellationToken cancellationToken)
    {
        var currentEmployee = await _context.Employees
            .FirstOrDefaultAsync(e => e.UserId.ToString() == CurrentUser.UserId, cancellationToken);

        if (currentEmployee == null)
            return Error.Unauthorized("User.NotLinkedToEmployee", "Current user is not linked to an employee");

        // Manager's own requests
        var myQuery = _context.EmployeeRequests
            .Include(r => r.RequestTypeRef)
            .Include(r => r.Employee).ThenInclude(e => e.Department)
            .Include(r => r.Employee).ThenInclude(e => e.JobTitle)
            .Include(r => r.Employee).ThenInclude(e => e.Branch)
            .Include(r => r.FeedbackDetail).ThenInclude(f => f!.FeedbackType)
            .Where(r => r.RequestTypeRef != null && r.RequestTypeRef.Code == "Feedback")
            .Where(r => r.EmployeeId == currentEmployee.Id)
            .OrderByDescending(r => r.RequestedDate);

        var myTotal = await myQuery.CountAsync(cancellationToken);
        var myItems = await myQuery
            .Skip((request.MyPageNumber - 1) * request.MyPageSize)
            .Take(request.MyPageSize)
            .Select(r => MapToDto(r))
            .ToListAsync(cancellationToken);

        // Pending approvals from direct reports
        var pendingQuery = _context.EmployeeRequests
            .Include(r => r.RequestTypeRef)
            .Include(r => r.Employee).ThenInclude(e => e.Department)
            .Include(r => r.Employee).ThenInclude(e => e.JobTitle)
            .Include(r => r.Employee).ThenInclude(e => e.Branch)
            .Include(r => r.FeedbackDetail).ThenInclude(f => f!.FeedbackType)
            .Where(r => r.RequestTypeRef != null && r.RequestTypeRef.Code == "Feedback")
            .Where(r => r.Status == EmployeeRequestStatus.Pending)
            .Where(r => r.Employee.DirectManagerId == currentEmployee.Id)
            .OrderByDescending(r => r.RequestedDate);

        var pendingTotal = await pendingQuery.CountAsync(cancellationToken);
        var pendingItems = await pendingQuery
            .Skip((request.PendingPageNumber - 1) * request.PendingPageSize)
            .Take(request.PendingPageSize)
            .Select(r => MapToDto(r))
            .ToListAsync(cancellationToken);

        var overview = new ManagerFeedbackOverviewDto
        {
            MyRequests = PagedResult<FeedbackRequestListDto>.Create(myItems, myTotal, request.MyPageNumber, request.MyPageSize),
            PendingApprovals = PagedResult<FeedbackRequestListDto>.Create(pendingItems, pendingTotal, request.PendingPageNumber, request.PendingPageSize)
        };

        return GenericResponse<ManagerFeedbackOverviewDto>.SuccessResult(overview, "Manager feedback overview retrieved successfully");
    }

    private static FeedbackRequestListDto MapToDto(Domain.Entities.Requests.EmployeeRequest r)
    {
        return new FeedbackRequestListDto
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
        };
    }
}
