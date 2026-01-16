using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Lifecycle.PolicyAcknowledgments.Queries.GetPolicyAcknowledgmentsList;

public class GetPolicyAcknowledgmentsListQueryHandler : IRequestHandler<GetPolicyAcknowledgmentsListQuery, ErrorOr<GenericResponse<PagedResult<PolicyAcknowledgmentListDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetPolicyAcknowledgmentsListQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<PagedResult<PolicyAcknowledgmentListDto>>>> Handle(
        GetPolicyAcknowledgmentsListQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.PolicyAcknowledgments
            .Include(p => p.Employee)
            .AsQueryable();

        query = query.ApplyFilters(
            request.EmployeeId,
            request.PolicyName,
            request.IsAcknowledged,
            request.AcknowledgedDateFrom,
            request.AcknowledgedDateTo);

        var totalCount = await query.CountAsync(cancellationToken);

        query = query.ApplySorting(request.SortBy, request.IsDescending);
        query = query.ApplyPaging(request.PageNumber, request.PageSize);

        var acknowledgments = await query.ToListAsync(cancellationToken);

        var dtos = acknowledgments.Select(p => new PolicyAcknowledgmentListDto
        {
            Id = p.Id,
            EmployeeId = p.EmployeeId,
            EmployeeName = p.Employee?.FullNameEn ?? string.Empty,
            PolicyName = p.PolicyName,
            PolicyVersion = p.PolicyVersion,
            AcknowledgedDate = p.AcknowledgedDate,
            IsAcknowledged = p.IsAcknowledged
        }).ToList();

        var pagedResult = new PagedResult<PolicyAcknowledgmentListDto>
        {
            Items = dtos,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };

        return new GenericResponse<PagedResult<PolicyAcknowledgmentListDto>>
        {
            Success = true,
            Data = pagedResult
        };
    }
}
