using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.Feedbacks.Queries.GetFeedbacksList;

public class GetFeedbacksListQueryHandler : IRequestHandler<GetFeedbacksListQuery, ErrorOr<GenericResponse<PagedResult<FeedbackListDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetFeedbacksListQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<PagedResult<FeedbackListDto>>>> Handle(
        GetFeedbacksListQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Feedbacks
            .Include(f => f.PerformanceReview)
                .ThenInclude(pr => pr.Employee)
            .Include(f => f.Provider)
            .AsQueryable();

        // Apply filters
        query = query.ApplyFilters(
            request.PerformanceReviewId,
            request.ProvidedBy,
            request.FeedbackType,
            request.IsAnonymous,
            request.RatingMin,
            request.RatingMax);

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting and pagination
        var feedbacks = await query
            .ApplySorting(request.SortBy, request.SortDescending)
            .ApplyPaging(request.PageNumber, request.PageSize)
            .ToListAsync(cancellationToken);

        // Map to DTOs
        var feedbackDtos = feedbacks.Select(f => new FeedbackListDto
        {
            Id = f.Id,
            PerformanceReviewId = f.PerformanceReviewId,
            EmployeeName = f.PerformanceReview.Employee.FullNameEn,
            ProviderName = f.IsAnonymous ? "Anonymous" : f.Provider.FullNameEn,
            FeedbackType = f.FeedbackType,
            Rating = f.Rating,
            IsAnonymous = f.IsAnonymous,
            CreatedDate = f.CreatedDate.DateTime
        }).ToList();

        var pagedResult = new PagedResult<FeedbackListDto>
        {
            Items = feedbackDtos,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };

        return new GenericResponse<PagedResult<FeedbackListDto>>
        {
            Success = true,
            Data = pagedResult
        };
    }
}
