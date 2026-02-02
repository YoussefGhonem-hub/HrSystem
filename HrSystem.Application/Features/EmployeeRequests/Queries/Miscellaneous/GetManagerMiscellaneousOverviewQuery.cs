using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.Miscellaneous;

public record GetManagerMiscellaneousOverviewQuery(
    int MyPageNumber = 1,
    int MyPageSize = 10,
    int PendingPageNumber = 1,
    int PendingPageSize = 10
) : IRequest<ErrorOr<GenericResponse<ManagerMiscellaneousOverviewDto>>>;

public class ManagerMiscellaneousOverviewDto
{
    public PagedResult<MiscellaneousRequestListDto> MyRequests { get; set; } = null!;
    public PagedResult<MiscellaneousRequestListDto> PendingApprovals { get; set; } = null!;
}

public class GetManagerMiscellaneousOverviewQueryHandler : IRequestHandler<GetManagerMiscellaneousOverviewQuery, ErrorOr<GenericResponse<ManagerMiscellaneousOverviewDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetManagerMiscellaneousOverviewQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<ManagerMiscellaneousOverviewDto>>> Handle(
        GetManagerMiscellaneousOverviewQuery request,
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
            .Include(r => r.MiscellaneousDetail).ThenInclude(m => m!.MiscellaneousType)
            .Where(r => r.RequestTypeRef != null && r.RequestTypeRef.Code == "Miscellaneous")
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
            .Include(r => r.MiscellaneousDetail).ThenInclude(m => m!.MiscellaneousType)
            .Where(r => r.RequestTypeRef != null && r.RequestTypeRef.Code == "Miscellaneous")
            .Where(r => r.Status == EmployeeRequestStatus.Pending)
            .Where(r => r.Employee.DirectManagerId == currentEmployee.Id)
            .OrderByDescending(r => r.RequestedDate);

        var pendingTotal = await pendingQuery.CountAsync(cancellationToken);
        var pendingItems = await pendingQuery
            .Skip((request.PendingPageNumber - 1) * request.PendingPageSize)
            .Take(request.PendingPageSize)
            .Select(r => MapToDto(r))
            .ToListAsync(cancellationToken);

        var overview = new ManagerMiscellaneousOverviewDto
        {
            MyRequests = PagedResult<MiscellaneousRequestListDto>.Create(myItems, myTotal, request.MyPageNumber, request.MyPageSize),
            PendingApprovals = PagedResult<MiscellaneousRequestListDto>.Create(pendingItems, pendingTotal, request.PendingPageNumber, request.PendingPageSize)
        };

        return GenericResponse<ManagerMiscellaneousOverviewDto>.SuccessResult(overview, "Manager miscellaneous overview retrieved successfully");
    }

    private static MiscellaneousRequestListDto MapToDto(Domain.Entities.Requests.EmployeeRequest r)
    {
        return new MiscellaneousRequestListDto
        {
            Id = r.Id,
            EmployeeId = r.EmployeeId,
            EmployeeCode = r.Employee.EmployeeCode,
            EmployeeName = r.Employee.FullNameEn,
            EmployeeNameAr = r.Employee.FullNameAr,
            DepartmentName = r.Employee.Department.NameEn,
            JobTitle = r.Employee.JobTitle.TitleEn,
            BranchName = r.Employee.Branch != null ? r.Employee.Branch.NameEn : null,
            MiscellaneousTypeId = r.MiscellaneousDetail!.MiscellaneousTypeId,
            MiscellaneousTypeName = r.MiscellaneousDetail.MiscellaneousType != null ? r.MiscellaneousDetail.MiscellaneousType.NameEn : null,
            MiscellaneousTypeNameAr = r.MiscellaneousDetail.MiscellaneousType != null ? r.MiscellaneousDetail.MiscellaneousType.NameAr : null,
            AdditionalNotes = r.MiscellaneousDetail.AdditionalNotes,
            ReferenceNumber = r.MiscellaneousDetail.ReferenceNumber,
            Priority = r.MiscellaneousDetail.Priority,
            ExpectedCompletionDate = r.MiscellaneousDetail.ExpectedCompletionDate,
            Title = r.Title,
            Description = r.Description,
            Status = r.Status,
            StatusName = r.Status.ToString(),
            RequestedDate = r.RequestedDate,
            StartDate = r.StartDate,
            EndDate = r.EndDate,
            ApprovedDate = r.ApprovedDate,
            RejectionReason = r.RejectionReason,
            CurrentApprovalLevel = r.Status == EmployeeRequestStatus.Pending ? 1 :
                                  r.Status == EmployeeRequestStatus.ManagerApproved ? 2 : 0
        };
    }
}
