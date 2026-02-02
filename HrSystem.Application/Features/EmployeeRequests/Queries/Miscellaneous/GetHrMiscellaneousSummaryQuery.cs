using ErrorOr;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.Miscellaneous;

public record GetHrMiscellaneousSummaryQuery(
    DateTime? StartDateFrom = null,
    DateTime? StartDateTo = null
) : IRequest<ErrorOr<GenericResponse<HrMiscellaneousSummaryDto>>>;

public class HrMiscellaneousSummaryDto
{
    public int TotalRequests { get; set; }
    public int PendingRequests { get; set; }
    public int ManagerApprovedRequests { get; set; }
    public int ApprovedRequests { get; set; }
    public int RejectedRequests { get; set; }
    public int CancelledRequests { get; set; }
}

public class GetHrMiscellaneousSummaryQueryHandler : IRequestHandler<GetHrMiscellaneousSummaryQuery, ErrorOr<GenericResponse<HrMiscellaneousSummaryDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetHrMiscellaneousSummaryQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<HrMiscellaneousSummaryDto>>> Handle(
        GetHrMiscellaneousSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var branchId = CurrentUser.BranchId;
        if (!branchId.HasValue)
            return Error.Unauthorized("User.NoBranch", "Current user is not associated with a branch");

        var query = _context.EmployeeRequests
            .Include(r => r.RequestTypeRef)
            .Include(r => r.Employee)
            .Where(r => r.RequestTypeRef != null && r.RequestTypeRef.Code == "Miscellaneous")
            .Where(r => r.Employee.BranchId == branchId.Value)
            .AsQueryable();

        if (request.StartDateFrom.HasValue)
            query = query.Where(r => r.StartDate >= request.StartDateFrom.Value);

        if (request.StartDateTo.HasValue)
            query = query.Where(r => r.StartDate <= request.StartDateTo.Value);

        var requests = await query.ToListAsync(cancellationToken);

        var summary = new HrMiscellaneousSummaryDto
        {
            TotalRequests = requests.Count,
            PendingRequests = requests.Count(r => r.Status == EmployeeRequestStatus.Pending),
            ManagerApprovedRequests = requests.Count(r => r.Status == EmployeeRequestStatus.ManagerApproved),
            ApprovedRequests = requests.Count(r => r.Status == EmployeeRequestStatus.Approved),
            RejectedRequests = requests.Count(r => r.Status == EmployeeRequestStatus.Rejected),
            CancelledRequests = requests.Count(r => r.Status == EmployeeRequestStatus.Cancelled)
        };

        return GenericResponse<HrMiscellaneousSummaryDto>.SuccessResult(summary, "HR miscellaneous summary retrieved successfully");
    }
}
