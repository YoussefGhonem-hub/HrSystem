using ErrorOr;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.Vacation;

public record GetHrVacationSummaryQuery(DateTime? StartDateFrom = null, DateTime? StartDateTo = null)
    : IRequest<ErrorOr<GenericResponse<HrVacationSummaryDto>>>;

public class GetHrVacationSummaryQueryHandler : IRequestHandler<GetHrVacationSummaryQuery, ErrorOr<GenericResponse<HrVacationSummaryDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetHrVacationSummaryQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<HrVacationSummaryDto>>> Handle(GetHrVacationSummaryQuery request, CancellationToken cancellationToken)
    {
        var branchId = CurrentUser.BranchId;
        var roles = CurrentUser.Roles ?? Array.Empty<string>();
        var isHr = roles.Contains(RoleNames.HRManager) || roles.Contains(RoleNames.OrganizationAdmin) || roles.Contains(RoleNames.HRSpecialist);

        if (!isHr)
        {
            return Error.Forbidden("Vacation.HROnly", "Only HR roles can access branch vacation summary");
        }

        if (!branchId.HasValue)
        {
            return Error.Conflict("Vacation.NoBranch", "Current HR user has no branch selected");
        }

        var baseQuery = _context.EmployeeRequests
            .Include(r => r.RequestTypeRef)
            .Include(r => r.Employee)
            .Where(r => r.RequestTypeRef != null && r.RequestTypeRef.Code == "Vacation")
            .Where(r => r.Employee.BranchId == branchId)
            .AsQueryable();

        if (request.StartDateFrom.HasValue)
        {
            baseQuery = baseQuery.Where(r => r.StartDate >= request.StartDateFrom.Value);
        }
        if (request.StartDateTo.HasValue)
        {
            baseQuery = baseQuery.Where(r => r.StartDate <= request.StartDateTo.Value);
        }

        var total = await baseQuery.CountAsync(cancellationToken);
        var pending = await baseQuery.CountAsync(r => r.Status == EmployeeRequestStatus.Pending, cancellationToken);
        var mgrApproved = await baseQuery.CountAsync(r => r.Status == EmployeeRequestStatus.ManagerApproved, cancellationToken);
        var approved = await baseQuery.CountAsync(r => r.Status == EmployeeRequestStatus.Approved, cancellationToken);
        var rejected = await baseQuery.CountAsync(r => r.Status == EmployeeRequestStatus.Rejected, cancellationToken);
        var cancelled = await baseQuery.CountAsync(r => r.Status == EmployeeRequestStatus.Cancelled, cancellationToken);

        var dto = new HrVacationSummaryDto
        {
            Total = total,
            Pending = pending,
            ManagerApproved = mgrApproved,
            Approved = approved,
            Rejected = rejected,
            Cancelled = cancelled
        };

        return new GenericResponse<HrVacationSummaryDto>
        {
            Success = true,
            Message = "HR vacation summary retrieved successfully",
            Data = dto
        };
    }
}

public class HrVacationSummaryDto
{
    public int Total { get; set; }
    public int Pending { get; set; }
    public int ManagerApproved { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
    public int Cancelled { get; set; }
}
