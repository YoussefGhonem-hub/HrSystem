using ErrorOr;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.Training;

public record GetHrTrainingSummaryQuery(
    DateTime? StartDateFrom = null,
    DateTime? StartDateTo = null
) : IRequest<ErrorOr<GenericResponse<HrTrainingSummaryDto>>>;

public class HrTrainingSummaryDto
{
    public int TotalRequests { get; set; }
    public int PendingRequests { get; set; }
    public int ManagerApprovedRequests { get; set; }
    public int ApprovedRequests { get; set; }
    public int RejectedRequests { get; set; }
    public int CancelledRequests { get; set; }
    public decimal TotalEstimatedCost { get; set; }
    public decimal TotalApprovedBudget { get; set; }
}

public class GetHrTrainingSummaryQueryHandler : IRequestHandler<GetHrTrainingSummaryQuery, ErrorOr<GenericResponse<HrTrainingSummaryDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetHrTrainingSummaryQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<HrTrainingSummaryDto>>> Handle(
        GetHrTrainingSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var branchId = CurrentUser.BranchId;
        if (!branchId.HasValue)
            return Error.Unauthorized("User.NoBranch", "Current user is not associated with a branch");

        var query = _context.EmployeeRequests
            .Include(r => r.RequestTypeRef)
            .Include(r => r.Employee)
            .Include(r => r.TrainingDetail)
            .Where(r => r.RequestTypeRef != null && r.RequestTypeRef.Code == "Training")
            .Where(r => r.Employee.BranchId == branchId.Value)
            .AsQueryable();

        if (request.StartDateFrom.HasValue)
            query = query.Where(r => r.TrainingDetail != null && r.TrainingDetail.TrainingStartDate >= request.StartDateFrom.Value);

        if (request.StartDateTo.HasValue)
            query = query.Where(r => r.TrainingDetail != null && r.TrainingDetail.TrainingStartDate <= request.StartDateTo.Value);

        var requests = await query.ToListAsync(cancellationToken);

        var summary = new HrTrainingSummaryDto
        {
            TotalRequests = requests.Count,
            PendingRequests = requests.Count(r => r.Status == EmployeeRequestStatus.Pending),
            ManagerApprovedRequests = requests.Count(r => r.Status == EmployeeRequestStatus.ManagerApproved),
            ApprovedRequests = requests.Count(r => r.Status == EmployeeRequestStatus.Approved),
            RejectedRequests = requests.Count(r => r.Status == EmployeeRequestStatus.Rejected),
            CancelledRequests = requests.Count(r => r.Status == EmployeeRequestStatus.Cancelled),
            TotalEstimatedCost = requests.Where(r => r.TrainingDetail?.EstimatedCost != null).Sum(r => r.TrainingDetail!.EstimatedCost!.Value),
            TotalApprovedBudget = requests.Where(r => r.TrainingDetail?.ApprovedBudget != null && r.Status == EmployeeRequestStatus.Approved).Sum(r => r.TrainingDetail!.ApprovedBudget!.Value)
        };

        return GenericResponse<HrTrainingSummaryDto>.SuccessResult(summary, "HR training summary retrieved successfully");
    }
}
