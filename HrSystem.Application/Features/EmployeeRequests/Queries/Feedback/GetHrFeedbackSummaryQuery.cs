using ErrorOr;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.Feedback;

public record GetHrFeedbackSummaryQuery(
    DateTime? StartDateFrom = null,
    DateTime? StartDateTo = null
) : IRequest<ErrorOr<GenericResponse<HrFeedbackSummaryDto>>>;

public class HrFeedbackSummaryDto
{
    public int TotalRequests { get; set; }
    public int PendingRequests { get; set; }
    public int ManagerApprovedRequests { get; set; }
    public int ApprovedRequests { get; set; }
    public int RejectedRequests { get; set; }
    public int CancelledRequests { get; set; }
    public int AnonymousFeedbacks { get; set; }
    public int ResponseRequired { get; set; }
    public int ResponseProvided { get; set; }
    public double? AverageRating { get; set; }
}

public class GetHrFeedbackSummaryQueryHandler : IRequestHandler<GetHrFeedbackSummaryQuery, ErrorOr<GenericResponse<HrFeedbackSummaryDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetHrFeedbackSummaryQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<HrFeedbackSummaryDto>>> Handle(
        GetHrFeedbackSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var branchId = CurrentUser.BranchId;
        if (!branchId.HasValue)
            return Error.Unauthorized("User.NoBranch", "Current user is not associated with a branch");

        var query = _context.EmployeeRequests
            .Include(r => r.RequestTypeRef)
            .Include(r => r.Employee)
            .Include(r => r.FeedbackDetail)
            .Where(r => r.RequestTypeRef != null && r.RequestTypeRef.Code == "Feedback")
            .Where(r => r.Employee.BranchId == branchId.Value)
            .AsQueryable();

        if (request.StartDateFrom.HasValue)
            query = query.Where(r => r.RequestedDate >= request.StartDateFrom.Value);

        if (request.StartDateTo.HasValue)
            query = query.Where(r => r.RequestedDate <= request.StartDateTo.Value);

        var requests = await query.ToListAsync(cancellationToken);

        var ratingsWithValue = requests.Where(r => r.FeedbackDetail?.Rating != null).Select(r => r.FeedbackDetail!.Rating!.Value).ToList();

        var summary = new HrFeedbackSummaryDto
        {
            TotalRequests = requests.Count,
            PendingRequests = requests.Count(r => r.Status == EmployeeRequestStatus.Pending),
            ManagerApprovedRequests = requests.Count(r => r.Status == EmployeeRequestStatus.ManagerApproved),
            ApprovedRequests = requests.Count(r => r.Status == EmployeeRequestStatus.Approved),
            RejectedRequests = requests.Count(r => r.Status == EmployeeRequestStatus.Rejected),
            CancelledRequests = requests.Count(r => r.Status == EmployeeRequestStatus.Cancelled),
            AnonymousFeedbacks = requests.Count(r => r.FeedbackDetail?.IsAnonymous == true),
            ResponseRequired = requests.Count(r => r.FeedbackDetail?.ResponseRequired == true),
            ResponseProvided = requests.Count(r => !string.IsNullOrEmpty(r.FeedbackDetail?.ResponseContent)),
            AverageRating = ratingsWithValue.Any() ? ratingsWithValue.Average() : null
        };

        return GenericResponse<HrFeedbackSummaryDto>.SuccessResult(summary, "HR feedback summary retrieved successfully");
    }
}
