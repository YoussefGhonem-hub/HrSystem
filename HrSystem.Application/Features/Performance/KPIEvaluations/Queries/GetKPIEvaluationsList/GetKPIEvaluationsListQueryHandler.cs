using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.KPIEvaluations.Queries.GetKPIEvaluationsList;

public class GetKPIEvaluationsListQueryHandler : IRequestHandler<GetKPIEvaluationsListQuery, ErrorOr<GenericResponse<PagedResult<KPIEvaluationListDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetKPIEvaluationsListQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<PagedResult<KPIEvaluationListDto>>>> Handle(
        GetKPIEvaluationsListQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.KPIEvaluations
            .Include(ke => ke.PerformanceReview)
                .ThenInclude(pr => pr.Employee)
            .Include(ke => ke.KPI)
            .AsQueryable();

        // Apply filters
        query = query.ApplyFilters(
            request.PerformanceReviewId,
            request.KPIId,
            request.RatingMin,
            request.RatingMax);

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting and pagination
        var kpiEvaluations = await query
            .ApplySorting(request.SortBy, request.SortDescending)
            .ApplyPaging(request.PageNumber, request.PageSize)
            .ToListAsync(cancellationToken);

        // Map to DTOs
        var kpiEvaluationDtos = kpiEvaluations.Select(ke => new KPIEvaluationListDto
        {
            Id = ke.Id,
            PerformanceReviewId = ke.PerformanceReviewId,
            EmployeeName = ke.PerformanceReview.Employee.FullNameEn,
            KPINameEn = ke.KPI.NameEn,
            KPINameAr = ke.KPI.NameAr,
            Rating = ke.Rating,
            WeightedScore = ke.WeightedScore,
            CreatedDate = ke.CreatedDate.DateTime
        }).ToList();

        var pagedResult = new PagedResult<KPIEvaluationListDto>
        {
            Items = kpiEvaluationDtos,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };

        return new GenericResponse<PagedResult<KPIEvaluationListDto>>
        {
            Success = true,
            Data = pagedResult
        };
    }
}
