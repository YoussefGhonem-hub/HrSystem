using ErrorOr;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.Permission;

public record GetManagerPermissionOverviewQuery(
    int MyPageNumber = 1,
    int MyPageSize = 10,
    int PendingPageNumber = 1,
    int PendingPageSize = 10
) : IRequest<ErrorOr<GenericResponse<ManagerPermissionOverviewDto>>>;

public class GetManagerPermissionOverviewQueryHandler : IRequestHandler<GetManagerPermissionOverviewQuery, ErrorOr<GenericResponse<ManagerPermissionOverviewDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetManagerPermissionOverviewQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<ManagerPermissionOverviewDto>>> Handle(GetManagerPermissionOverviewQuery request, CancellationToken cancellationToken)
    {
        var roles = CurrentUser.Roles ?? Array.Empty<string>();
        var isManager = roles.Contains(RoleNames.DepartmentManager) || roles.Contains(RoleNames.OrganizationAdmin);
        if (!isManager)
        {
            return Error.Forbidden("Permission.ManagerOnly", "Only department managers can access this overview");
        }

        var currentEmployee = await _context.Employees
            .FirstOrDefaultAsync(e => e.UserId.ToString() == CurrentUser.UserId, cancellationToken);
        if (currentEmployee == null)
            return Error.Unauthorized("User.NotLinkedToEmployee", "Current user is not linked to an employee");

        // My own requests
        var myQuery = _context.EmployeeRequests
            .Include(r => r.RequestTypeRef)
            .Include(r => r.Employee).ThenInclude(e => e.Department)
            .Include(r => r.Employee).ThenInclude(e => e.JobTitle)
            .Include(r => r.Employee).ThenInclude(e => e.Branch)
            .Include(r => r.PermissionDetail).ThenInclude(p => p!.PermissionType)
            .Where(r => r.RequestTypeRef != null && r.RequestTypeRef.Code == "Permission")
            .Where(r => r.EmployeeId == currentEmployee.Id)
            .OrderByDescending(r => r.CreatedDate)
            .AsQueryable();

        var myItems = await myQuery
            .Skip((request.MyPageNumber - 1) * request.MyPageSize)
            .Take(request.MyPageSize)
            .Select(r => new PermissionRequestListDto
            {
                Id = r.Id,
                EmployeeId = r.EmployeeId,
                EmployeeCode = r.Employee.EmployeeCode,
                EmployeeName = r.Employee.FullNameEn,
                EmployeeNameAr = r.Employee.FullNameAr,
                DepartmentName = r.Employee.Department.NameEn,
                JobTitle = r.Employee.JobTitle.TitleEn,
                BranchName = r.Employee.Branch != null ? r.Employee.Branch.NameEn : null,
                PermissionTypeId = r.PermissionDetail!.PermissionTypeId,
                PermissionTypeName = r.PermissionDetail.PermissionType != null ? r.PermissionDetail.PermissionType.NameEn : null,
                PermissionTypeNameAr = r.PermissionDetail.PermissionType != null ? r.PermissionDetail.PermissionType.NameAr : null,
                PermissionDate = r.PermissionDetail.PermissionDate,
                FromTime = r.PermissionDetail.FromTime,
                ToTime = r.PermissionDetail.ToTime,
                TotalHours = r.PermissionDetail.TotalHours,
                Reason = r.PermissionDetail.Reason,
                Title = r.Title,
                Description = r.Description,
                Status = r.Status,
                StatusName = r.Status.ToString(),
                ManagerApprovalDate = r.PermissionDetail.ManagerApprovalDate,
                ManagerComments = r.PermissionDetail.ManagerComments,
                AttachmentUrl = r.AttachmentUrl,
                CreatedDate = r.CreatedDate,
                CurrentApprovalLevel = r.Status == EmployeeRequestStatus.Pending ? "Manager" :
                                       r.Status == EmployeeRequestStatus.ManagerApproved ? "HR" : "Completed"
            })
            .ToListAsync(cancellationToken);

        // Pending approvals from direct reports
        var pendingQuery = _context.EmployeeRequests
            .Include(r => r.RequestTypeRef)
            .Include(r => r.Employee).ThenInclude(e => e.Department)
            .Include(r => r.Employee).ThenInclude(e => e.JobTitle)
            .Include(r => r.Employee).ThenInclude(e => e.Branch)
            .Include(r => r.PermissionDetail).ThenInclude(p => p!.PermissionType)
            .Where(r => r.RequestTypeRef != null && r.RequestTypeRef.Code == "Permission")
            .Where(r => r.Status == EmployeeRequestStatus.Pending && r.Employee.DirectManagerId == currentEmployee.Id)
            .OrderBy(r => r.PermissionDetail!.PermissionDate)
            .AsQueryable();

        var pendingItems = await pendingQuery
            .Skip((request.PendingPageNumber - 1) * request.PendingPageSize)
            .Take(request.PendingPageSize)
            .Select(r => new PermissionRequestListDto
            {
                Id = r.Id,
                EmployeeId = r.EmployeeId,
                EmployeeCode = r.Employee.EmployeeCode,
                EmployeeName = r.Employee.FullNameEn,
                EmployeeNameAr = r.Employee.FullNameAr,
                DepartmentName = r.Employee.Department.NameEn,
                JobTitle = r.Employee.JobTitle.TitleEn,
                BranchName = r.Employee.Branch != null ? r.Employee.Branch.NameEn : null,
                PermissionTypeId = r.PermissionDetail!.PermissionTypeId,
                PermissionTypeName = r.PermissionDetail.PermissionType != null ? r.PermissionDetail.PermissionType.NameEn : null,
                PermissionTypeNameAr = r.PermissionDetail.PermissionType != null ? r.PermissionDetail.PermissionType.NameAr : null,
                PermissionDate = r.PermissionDetail.PermissionDate,
                FromTime = r.PermissionDetail.FromTime,
                ToTime = r.PermissionDetail.ToTime,
                TotalHours = r.PermissionDetail.TotalHours,
                Reason = r.PermissionDetail.Reason,
                Title = r.Title,
                Description = r.Description,
                Status = r.Status,
                StatusName = r.Status.ToString(),
                ManagerApprovalDate = r.PermissionDetail.ManagerApprovalDate,
                ManagerComments = r.PermissionDetail.ManagerComments,
                AttachmentUrl = r.AttachmentUrl,
                CreatedDate = r.CreatedDate,
                CurrentApprovalLevel = r.Status == EmployeeRequestStatus.Pending ? "Manager" :
                                       r.Status == EmployeeRequestStatus.ManagerApproved ? "HR" : "Completed"
            })
            .ToListAsync(cancellationToken);

        var dto = new ManagerPermissionOverviewDto
        {
            MyRequests = myItems,
            PendingApprovals = pendingItems
        };

        return new GenericResponse<ManagerPermissionOverviewDto>
        {
            Success = true,
            Message = "Manager permission overview retrieved successfully",
            Data = dto
        };
    }
}

public class ManagerPermissionOverviewDto
{
    public List<PermissionRequestListDto> MyRequests { get; set; } = new();
    public List<PermissionRequestListDto> PendingApprovals { get; set; } = new();
}
