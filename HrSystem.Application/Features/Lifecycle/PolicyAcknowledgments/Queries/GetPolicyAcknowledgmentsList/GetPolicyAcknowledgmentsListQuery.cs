using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Lifecycle.PolicyAcknowledgments.Queries.GetPolicyAcknowledgmentsList;

public record GetPolicyAcknowledgmentsListQuery(
    Guid? EmployeeId = null,
    string? PolicyName = null,
    bool? IsAcknowledged = null,
    DateTime? AcknowledgedDateFrom = null,
    DateTime? AcknowledgedDateTo = null,
    string? SortBy = null,
    bool IsDescending = false,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<ErrorOr<GenericResponse<PagedResult<PolicyAcknowledgmentListDto>>>>;
