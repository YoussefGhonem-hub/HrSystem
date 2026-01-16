using FluentValidation;

namespace HrSystem.Application.Leave.Queries.GetPendingLeaveRequests;

public class GetPendingLeaveRequestsQueryValidator : AbstractValidator<GetPendingLeaveRequestsQuery>
{
    public GetPendingLeaveRequestsQueryValidator()
    {
        // No validation rules needed for this query as it has no parameters
        // This validator exists to maintain consistency in the application structure
    }
}
