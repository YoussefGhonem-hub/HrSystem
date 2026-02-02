using ErrorOr;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.Vacation;

public record GetManagerVacationOverviewQuery(
    int MyPageNumber = 1,
    int MyPageSize = 10,
    int PendingPageNumber = 1,
    int PendingPageSize = 10
) : IRequest<ErrorOr<GenericResponse<ManagerVacationOverviewDto>>>;

public class GetManagerVacationOverviewQueryHandler : IRequestHandler<GetManagerVacationOverviewQuery, ErrorOr<GenericResponse<ManagerVacationOverviewDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetManagerVacationOverviewQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<ManagerVacationOverviewDto>>> Handle(GetManagerVacationOverviewQuery request, CancellationToken cancellationToken)
    {
        var roles = CurrentUser.Roles ?? Array.Empty<string>();
        var isManager = roles.Contains(RoleNames.DepartmentManager) || roles.Contains(RoleNames.OrganizationAdmin);
        if (!isManager)
        {
            return Error.Forbidden("Vacation.ManagerOnly", "Only department managers can access this overview");
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
            .Include(r => r.VacationDetail).ThenInclude(v => v!.VacationType)
            .Where(r => r.RequestTypeRef != null && r.RequestTypeRef.Code == "Vacation")
            .Where(r => r.EmployeeId == currentEmployee.Id)
            .OrderByDescending(r => r.CreatedDate)
            .AsQueryable();

        var myItems = await myQuery
            .Skip((request.MyPageNumber - 1) * request.MyPageSize)
            .Take(request.MyPageSize)
            .Select(r => new VacationRequestListDto
            {
                Id = r.Id,
                EmployeeId = r.EmployeeId,
                EmployeeCode = r.Employee.EmployeeCode,
                EmployeeName = r.Employee.FullNameEn,
                EmployeeNameAr = r.Employee.FullNameAr,
                DepartmentName = r.Employee.Department.NameEn,
                JobTitle = r.Employee.JobTitle.TitleEn,
                BranchName = r.Employee.Branch != null ? r.Employee.Branch.NameEn : null,
                VacationTypeId = r.VacationDetail!.VacationTypeId,
                VacationTypeName = r.VacationDetail.VacationType != null ? r.VacationDetail.VacationType.NameEn : null,
                VacationTypeNameAr = r.VacationDetail.VacationType != null ? r.VacationDetail.VacationType.NameAr : null,
                StartDate = r.StartDate ?? DateTime.MinValue,
                EndDate = r.EndDate ?? DateTime.MinValue,
                TotalDays = r.VacationDetail.TotalDays,
                Title = r.Title,
                Description = r.Description,
                Status = r.Status,
                StatusName = r.Status.ToString(),
                ManagerApprovalDate = r.VacationDetail.ManagerApprovalDate,
                ManagerComments = r.VacationDetail.ManagerComments,
                HRApprovalDate = r.VacationDetail.HRApprovalDate,
                HRComments = r.VacationDetail.HRComments,
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
            .Include(r => r.VacationDetail).ThenInclude(v => v!.VacationType)
            .Where(r => r.RequestTypeRef != null && r.RequestTypeRef.Code == "Vacation")
            .Where(r => r.Status == EmployeeRequestStatus.Pending && r.Employee.DirectManagerId == currentEmployee.Id)
            .OrderBy(r => r.StartDate)
            .AsQueryable();

        var pendingItems = await pendingQuery
            .Skip((request.PendingPageNumber - 1) * request.PendingPageSize)
            .Take(request.PendingPageSize)
            .Select(r => new VacationRequestListDto
            {
                Id = r.Id,
                EmployeeId = r.EmployeeId,
                EmployeeCode = r.Employee.EmployeeCode,
                EmployeeName = r.Employee.FullNameEn,
                EmployeeNameAr = r.Employee.FullNameAr,
                DepartmentName = r.Employee.Department.NameEn,
                JobTitle = r.Employee.JobTitle.TitleEn,
                BranchName = r.Employee.Branch != null ? r.Employee.Branch.NameEn : null,
                VacationTypeId = r.VacationDetail!.VacationTypeId,
                VacationTypeName = r.VacationDetail.VacationType != null ? r.VacationDetail.VacationType.NameEn : null,
                VacationTypeNameAr = r.VacationDetail.VacationType != null ? r.VacationDetail.VacationType.NameAr : null,
                StartDate = r.StartDate ?? DateTime.MinValue,
                EndDate = r.EndDate ?? DateTime.MinValue,
                TotalDays = r.VacationDetail.TotalDays,
                Title = r.Title,
                Description = r.Description,
                Status = r.Status,
                StatusName = r.Status.ToString(),
                ManagerApprovalDate = r.VacationDetail.ManagerApprovalDate,
                ManagerComments = r.VacationDetail.ManagerComments,
                HRApprovalDate = r.VacationDetail.HRApprovalDate,
                HRComments = r.VacationDetail.HRComments,
                AttachmentUrl = r.AttachmentUrl,
                CreatedDate = r.CreatedDate,
                CurrentApprovalLevel = r.Status == EmployeeRequestStatus.Pending ? "Manager" :
                                       r.Status == EmployeeRequestStatus.ManagerApproved ? "HR" : "Completed"
            })
            .ToListAsync(cancellationToken);

        var dto = new ManagerVacationOverviewDto
        {
            MyRequests = myItems,
            PendingApprovals = pendingItems
        };

        return new GenericResponse<ManagerVacationOverviewDto>
        {
            Success = true,
            Message = "Manager vacation overview retrieved successfully",
            Data = dto
        };
    }
}

public class ManagerVacationOverviewDto
{
    public List<VacationRequestListDto> MyRequests { get; set; } = new();
    public List<VacationRequestListDto> PendingApprovals { get; set; } = new();
}
