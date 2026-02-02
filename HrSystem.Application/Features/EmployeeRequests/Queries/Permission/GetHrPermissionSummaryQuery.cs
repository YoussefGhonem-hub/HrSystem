using ErrorOr;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.Permission;

public record GetHrPermissionSummaryQuery(DateTime? StartDateFrom = null, DateTime? StartDateTo = null)
    : IRequest<ErrorOr<GenericResponse<HrPermissionSummaryDto>>>;

public class GetHrPermissionSummaryQueryHandler : IRequestHandler<GetHrPermissionSummaryQuery, ErrorOr<GenericResponse<HrPermissionSummaryDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetHrPermissionSummaryQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<HrPermissionSummaryDto>>> Handle(GetHrPermissionSummaryQuery request, CancellationToken cancellationToken)
    {
        var branchId = CurrentUser.BranchId;
        var roles = CurrentUser.Roles ?? Array.Empty<string>();
        var isHr = roles.Contains(RoleNames.HRManager) || roles.Contains(RoleNames.OrganizationAdmin) || roles.Contains(RoleNames.HRSpecialist);

        if (!isHr)
        {
            return Error.Forbidden("Permission.HROnly", "Only HR roles can access branch permission summary");
        }

        if (!branchId.HasValue)
        {
            return Error.Conflict("Permission.NoBranch", "Current HR user has no branch selected");
        }

        var baseQuery = _context.EmployeeRequests
            .Include(r => r.RequestTypeRef)
            .Include(r => r.Employee)
            .Include(r => r.PermissionDetail)
            .Where(r => r.RequestTypeRef != null && r.RequestTypeRef.Code == "Permission")
            .Where(r => r.Employee.BranchId == branchId)
            .AsQueryable();

        if (request.StartDateFrom.HasValue)
        {
            baseQuery = baseQuery.Where(r => r.PermissionDetail != null && r.PermissionDetail.PermissionDate >= request.StartDateFrom.Value);
        }
        if (request.StartDateTo.HasValue)
        {
            baseQuery = baseQuery.Where(r => r.PermissionDetail != null && r.PermissionDetail.PermissionDate <= request.StartDateTo.Value);
        }

        var total = await baseQuery.CountAsync(cancellationToken);
        var pending = await baseQuery.CountAsync(r => r.Status == EmployeeRequestStatus.Pending, cancellationToken);
        var mgrApproved = await baseQuery.CountAsync(r => r.Status == EmployeeRequestStatus.ManagerApproved, cancellationToken);
        var approved = await baseQuery.CountAsync(r => r.Status == EmployeeRequestStatus.Approved, cancellationToken);
        var rejected = await baseQuery.CountAsync(r => r.Status == EmployeeRequestStatus.Rejected, cancellationToken);
        var cancelled = await baseQuery.CountAsync(r => r.Status == EmployeeRequestStatus.Cancelled, cancellationToken);

        // Total hours approved this period
        var totalHoursApproved = await baseQuery
            .Where(r => r.Status == EmployeeRequestStatus.Approved && r.PermissionDetail != null)
            .SumAsync(r => r.PermissionDetail!.TotalHours, cancellationToken);

        var dto = new HrPermissionSummaryDto
        {
            Total = total,
            Pending = pending,
            ManagerApproved = mgrApproved,
            Approved = approved,
            Rejected = rejected,
            Cancelled = cancelled,
            TotalHoursApproved = totalHoursApproved
        };

        return new GenericResponse<HrPermissionSummaryDto>
        {
            Success = true,
            Message = "HR permission summary retrieved successfully",
            Data = dto
        };
    }
}

public class HrPermissionSummaryDto
{
    public int Total { get; set; }
    public int Pending { get; set; }
    public int ManagerApproved { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
    public int Cancelled { get; set; }
    public decimal TotalHoursApproved { get; set; }
}
