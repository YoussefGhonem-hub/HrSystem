using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.PerformanceReviews.Queries.GetPerformanceReviewsList;

public class GetPerformanceReviewsListQueryHandler : IRequestHandler<GetPerformanceReviewsListQuery, ErrorOr<GenericResponse<PagedResult<PerformanceReviewListDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetPerformanceReviewsListQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<PagedResult<PerformanceReviewListDto>>>> Handle(
        GetPerformanceReviewsListQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.PerformanceReviews
            .Include(r => r.Employee)
            .Include(r => r.Reviewer)
            .AsQueryable();

        query = query.ApplyFilters(
            request.EmployeeId,
            request.ReviewerId,
            request.ReviewType,
            request.Status,
            request.ReviewDateFrom,
            request.ReviewDateTo,
            request.EmployeeAcknowledged);

        var totalCount = await query.CountAsync(cancellationToken);

        query = query.ApplySorting(request.SortBy, request.IsDescending);
        query = query.ApplyPaging(request.PageNumber, request.PageSize);

        var reviews = await query.ToListAsync(cancellationToken);

        var dtos = reviews.Select(r => new PerformanceReviewListDto
        {
            Id = r.Id,
            EmployeeId = r.EmployeeId,
            EmployeeName = r.Employee?.FullNameEn ?? string.Empty,
            ReviewerId = r.ReviewerId,
            ReviewerName = r.Reviewer?.FullNameEn ?? string.Empty,
            ReviewPeriodStart = r.ReviewPeriodStart,
            ReviewPeriodEnd = r.ReviewPeriodEnd,
            ReviewDate = r.ReviewDate,
            ReviewType = r.ReviewType,
            OverallRating = r.OverallRating,
            Status = r.Status,
            EmployeeAcknowledged = r.EmployeeAcknowledged
        }).ToList();

        var pagedResult = new PagedResult<PerformanceReviewListDto>
        {
            Items = dtos,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };

        return new GenericResponse<PagedResult<PerformanceReviewListDto>>
        {
            Success = true,
            Data = pagedResult
        };
    }
}
