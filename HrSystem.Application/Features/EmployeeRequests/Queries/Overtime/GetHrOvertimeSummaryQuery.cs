using ErrorOr;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.Overtime;

public record GetHrOvertimeSummaryQuery(
    DateTime? StartDateFrom = null,
    DateTime? StartDateTo = null
) : IRequest<ErrorOr<GenericResponse<HrOvertimeSummaryDto>>>;

public class HrOvertimeSummaryDto
{
    public int TotalRequests { get; set; }
    public int PendingRequests { get; set; }
    public int ManagerApprovedRequests { get; set; }
    public int ApprovedRequests { get; set; }
    public int RejectedRequests { get; set; }
    public int CancelledRequests { get; set; }
    public double TotalPlannedHours { get; set; }
    public double TotalActualHours { get; set; }
}

public class GetHrOvertimeSummaryQueryHandler
    : IRequestHandler<GetHrOvertimeSummaryQuery, ErrorOr<GenericResponse<HrOvertimeSummaryDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetHrOvertimeSummaryQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<HrOvertimeSummaryDto>>> Handle(
        GetHrOvertimeSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var branchId = CurrentUser.BranchId;
        if (!branchId.HasValue)
            return Error.Unauthorized("User.NoBranch", "Current user is not associated with a branch");

        var query = _context.EmployeeRequests
            .Include(r => r.RequestTypeRef)
            .Include(r => r.Employee)
            .Include(r => r.OvertimeDetail)
            .Where(r => r.RequestTypeRef != null && r.RequestTypeRef.Code == "OverTime")
            .Where(r => r.Employee.BranchId == branchId.Value)
            .AsQueryable();

        if (request.StartDateFrom.HasValue)
            query = query.Where(r => r.OvertimeDetail != null && r.OvertimeDetail.OvertimeDate >= request.StartDateFrom.Value);

        if (request.StartDateTo.HasValue)
            query = query.Where(r => r.OvertimeDetail != null && r.OvertimeDetail.OvertimeDate <= request.StartDateTo.Value);

        var requests = await query.ToListAsync(cancellationToken);

        var summary = new HrOvertimeSummaryDto
        {
            TotalRequests = requests.Count,
            PendingRequests = requests.Count(r => r.Status == EmployeeRequestStatus.Pending),
            ManagerApprovedRequests = requests.Count(r => r.Status == EmployeeRequestStatus.ManagerApproved),
            ApprovedRequests = requests.Count(r => r.Status == EmployeeRequestStatus.Approved),
            RejectedRequests = requests.Count(r => r.Status == EmployeeRequestStatus.Rejected),
            CancelledRequests = requests.Count(r => r.Status == EmployeeRequestStatus.Cancelled),
            TotalPlannedHours = requests
                .Where(r => r.OvertimeDetail != null)
                .Sum(r => r.OvertimeDetail!.PlannedHours.TotalHours),
            TotalActualHours = requests
                .Where(r => r.OvertimeDetail?.ActualHours != null && r.Status == EmployeeRequestStatus.Approved)
                .Sum(r => r.OvertimeDetail!.ActualHours!.Value.TotalHours)
        };

        return GenericResponse<HrOvertimeSummaryDto>.SuccessResult(summary, "HR overtime summary retrieved successfully");
    }
}
