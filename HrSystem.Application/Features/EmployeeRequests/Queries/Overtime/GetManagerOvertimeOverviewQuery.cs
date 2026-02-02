using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.Overtime;

public record GetManagerOvertimeOverviewQuery(
    int MyPageNumber = 1,
    int MyPageSize = 10,
    int PendingPageNumber = 1,
    int PendingPageSize = 10
) : IRequest<ErrorOr<GenericResponse<ManagerOvertimeOverviewDto>>>;

public class ManagerOvertimeOverviewDto
{
    public PagedResult<OvertimeRequestListDto> MyRequests { get; set; } = null!;
    public PagedResult<OvertimeRequestListDto> PendingApprovals { get; set; } = null!;
}

public class GetManagerOvertimeOverviewQueryHandler : IRequestHandler<GetManagerOvertimeOverviewQuery, ErrorOr<GenericResponse<ManagerOvertimeOverviewDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetManagerOvertimeOverviewQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<ManagerOvertimeOverviewDto>>> Handle(
        GetManagerOvertimeOverviewQuery request,
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
            .Include(r => r.OvertimeDetail).ThenInclude(o => o!.OvertimeType)
            .Where(r => r.RequestTypeRef != null && r.RequestTypeRef.Code == "OverTime")
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
            .Include(r => r.OvertimeDetail).ThenInclude(o => o!.OvertimeType)
            .Where(r => r.RequestTypeRef != null && r.RequestTypeRef.Code == "OverTime")
            .Where(r => r.Status == EmployeeRequestStatus.Pending)
            .Where(r => r.Employee.DirectManagerId == currentEmployee.Id)
            .OrderByDescending(r => r.RequestedDate);

        var pendingTotal = await pendingQuery.CountAsync(cancellationToken);
        var pendingItems = await pendingQuery
            .Skip((request.PendingPageNumber - 1) * request.PendingPageSize)
            .Take(request.PendingPageSize)
            .Select(r => MapToDto(r))
            .ToListAsync(cancellationToken);

        var overview = new ManagerOvertimeOverviewDto
        {
            MyRequests = PagedResult<OvertimeRequestListDto>.Create(myItems, myTotal, request.MyPageNumber, request.MyPageSize),
            PendingApprovals = PagedResult<OvertimeRequestListDto>.Create(pendingItems, pendingTotal, request.PendingPageNumber, request.PendingPageSize)
        };

        return GenericResponse<ManagerOvertimeOverviewDto>.SuccessResult(overview, "Manager overtime overview retrieved successfully");
    }

    private static OvertimeRequestListDto MapToDto(Domain.Entities.Requests.EmployeeRequest r)
    {
        return new OvertimeRequestListDto
        {
            Id = r.Id,
            EmployeeId = r.EmployeeId,
            EmployeeCode = r.Employee.EmployeeCode,
            EmployeeName = r.Employee.FullNameEn,
            EmployeeNameAr = r.Employee.FullNameAr,
            DepartmentName = r.Employee.Department.NameEn,
            JobTitle = r.Employee.JobTitle.TitleEn,
            BranchName = r.Employee.Branch != null ? r.Employee.Branch.NameEn : null,
            OvertimeTypeId = r.OvertimeDetail!.OvertimeTypeId,
            OvertimeTypeName = r.OvertimeDetail.OvertimeType != null ? r.OvertimeDetail.OvertimeType.NameEn : null,
            OvertimeTypeNameAr = r.OvertimeDetail.OvertimeType != null ? r.OvertimeDetail.OvertimeType.NameAr : null,
            OvertimeDate = r.OvertimeDetail.OvertimeDate,
            PlannedHours = r.OvertimeDetail.PlannedHours,
            ActualHours = r.OvertimeDetail.ActualHours,
            Multiplier = r.OvertimeDetail.Multiplier,
            ProjectCode = r.OvertimeDetail.ProjectCode,
            TaskDescription = r.OvertimeDetail.TaskDescription,
            Title = r.Title,
            Description = r.Description,
            Status = r.Status,
            StatusName = r.Status.ToString(),
            RequestedDate = r.RequestedDate,
            ApprovedDate = r.OvertimeDetail.ApprovedDate,
            ApprovalNotes = r.OvertimeDetail.ApprovalNotes,
            RejectionReason = r.RejectionReason,
            CurrentApprovalLevel = r.Status == EmployeeRequestStatus.Pending ? 1 :
                                  r.Status == EmployeeRequestStatus.ManagerApproved ? 2 : 0
        };
    }
}
