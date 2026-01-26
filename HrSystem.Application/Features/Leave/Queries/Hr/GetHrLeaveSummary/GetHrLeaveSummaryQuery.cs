using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Leave.Queries.Hr.GetHrLeaveSummary;

public record GetHrLeaveSummaryQuery(DateTime? StartDateFrom = null, DateTime? StartDateTo = null)
    : IRequest<ErrorOr<GenericResponse<HrLeaveSummaryDto>>>;

public class GetHrLeaveSummaryQueryHandler : IRequestHandler<GetHrLeaveSummaryQuery, ErrorOr<GenericResponse<HrLeaveSummaryDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetHrLeaveSummaryQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<HrLeaveSummaryDto>>> Handle(GetHrLeaveSummaryQuery request, CancellationToken cancellationToken)
    {
        var branchId = CurrentUser.BranchId;
        var roles = CurrentUser.Roles ?? Array.Empty<string>();
        var isHr = roles.Contains(RoleNames.HRManager) || roles.Contains(RoleNames.OrganizationAdmin) || roles.Contains(RoleNames.HRSpecialist);

        if (!isHr)
        {
            return Error.Forbidden("Leave.HROnly", "Only HR roles can access branch leave summary");
        }

        if (!branchId.HasValue)
        {
            return Error.Conflict("Leave.NoBranch", "Current HR user has no branch selected");
        }

        var baseQuery = _context.LeaveRequests
            .Include(lr => lr.Employee)
            .Where(lr => lr.Employee.BranchId == branchId)
            .AsQueryable();

        if (request.StartDateFrom.HasValue)
        {
            baseQuery = baseQuery.Where(lr => lr.StartDate >= request.StartDateFrom.Value);
        }
        if (request.StartDateTo.HasValue)
        {
            baseQuery = baseQuery.Where(lr => lr.StartDate <= request.StartDateTo.Value);
        }

        var total = await baseQuery.CountAsync(cancellationToken);
        var pending = await baseQuery.CountAsync(lr => lr.LeaveStatusId == LeaveStatusIds.Pending, cancellationToken);
        var mgrApproved = await baseQuery.CountAsync(lr => lr.LeaveStatusId == LeaveStatusIds.ManagerApproved, cancellationToken);
        var hrApproved = await baseQuery.CountAsync(lr => lr.LeaveStatusId == LeaveStatusIds.HRApproved, cancellationToken);
        var approved = await baseQuery.CountAsync(lr => lr.LeaveStatusId == LeaveStatusIds.Approved, cancellationToken);
        var rejected = await baseQuery.CountAsync(lr => lr.LeaveStatusId == LeaveStatusIds.Rejected, cancellationToken);
        var cancelled = await baseQuery.CountAsync(lr => lr.LeaveStatusId == LeaveStatusIds.Cancelled, cancellationToken);

        var dto = new HrLeaveSummaryDto
        {
            Total = total,
            Pending = pending,
            ManagerApproved = mgrApproved,
            HRApproved = hrApproved,
            Approved = approved,
            Rejected = rejected,
            Cancelled = cancelled
        };

        return new GenericResponse<HrLeaveSummaryDto>
        {
            Success = true,
            Message = "HR leave summary retrieved successfully",
            Data = dto
        };
    }
}
