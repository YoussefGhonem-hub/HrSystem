using ErrorOr;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.Personal;

public record GetHrPersonalSummaryQuery(
    DateTime? StartDateFrom = null,
    DateTime? StartDateTo = null
) : IRequest<ErrorOr<GenericResponse<HrPersonalSummaryDto>>>;

public class HrPersonalSummaryDto
{
    public int TotalRequests { get; set; }
    public int PendingRequests { get; set; }
    public int ManagerApprovedRequests { get; set; }
    public int ApprovedRequests { get; set; }
    public int RejectedRequests { get; set; }
    public int CancelledRequests { get; set; }
    public int UrgentRequests { get; set; }
    public int ConfidentialRequests { get; set; }
}

public class GetHrPersonalSummaryQueryHandler : IRequestHandler<GetHrPersonalSummaryQuery, ErrorOr<GenericResponse<HrPersonalSummaryDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetHrPersonalSummaryQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<HrPersonalSummaryDto>>> Handle(
        GetHrPersonalSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var branchId = CurrentUser.BranchId;
        if (!branchId.HasValue)
            return Error.Unauthorized("User.NoBranch", "Current user is not associated with a branch");

        var query = _context.EmployeeRequests
            .Include(r => r.RequestTypeRef)
            .Include(r => r.Employee)
            .Include(r => r.PersonalDetail)
            .Where(r => r.RequestTypeRef != null && r.RequestTypeRef.Code == "Personal")
            .Where(r => r.Employee.BranchId == branchId.Value)
            .AsQueryable();

        if (request.StartDateFrom.HasValue)
            query = query.Where(r => r.StartDate >= request.StartDateFrom.Value);

        if (request.StartDateTo.HasValue)
            query = query.Where(r => r.StartDate <= request.StartDateTo.Value);

        var requests = await query.ToListAsync(cancellationToken);

        var summary = new HrPersonalSummaryDto
        {
            TotalRequests = requests.Count,
            PendingRequests = requests.Count(r => r.Status == EmployeeRequestStatus.Pending),
            ManagerApprovedRequests = requests.Count(r => r.Status == EmployeeRequestStatus.ManagerApproved),
            ApprovedRequests = requests.Count(r => r.Status == EmployeeRequestStatus.Approved),
            RejectedRequests = requests.Count(r => r.Status == EmployeeRequestStatus.Rejected),
            CancelledRequests = requests.Count(r => r.Status == EmployeeRequestStatus.Cancelled),
            UrgentRequests = requests.Count(r => r.PersonalDetail?.IsUrgent == true),
            ConfidentialRequests = requests.Count(r => r.PersonalDetail?.RequiresConfidentiality == true)
        };

        return GenericResponse<HrPersonalSummaryDto>.SuccessResult(summary, "HR personal requests summary retrieved successfully");
    }
}
