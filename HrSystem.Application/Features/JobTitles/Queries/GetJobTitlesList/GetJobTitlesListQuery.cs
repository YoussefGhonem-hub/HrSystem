using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.JobTitles.Queries.GetJobTitlesList;

public record GetJobTitlesListQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    string? SortBy = null,
    bool SortDescending = false
) : IRequest<ErrorOr<GenericResponse<PagedResult<JobTitleListDto>>>>;

public class GetJobTitlesListQueryHandler : IRequestHandler<GetJobTitlesListQuery, ErrorOr<GenericResponse<PagedResult<JobTitleListDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetJobTitlesListQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<PagedResult<JobTitleListDto>>>> Handle(
        GetJobTitlesListQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.JobTitles
            .Include(j => j.Employees)
            .AsQueryable();

        query = query.ApplyFilters(request.SearchTerm);

        var totalCount = await query.CountAsync(cancellationToken);

        var jobTitles = await query
            .ApplySorting(request.SortBy, request.SortDescending)
            .ApplyPaging(request.PageNumber, request.PageSize)
            .ToListAsync(cancellationToken);

        var jobTitleDtos = jobTitles.Adapt<List<JobTitleListDto>>();

        var pagedResult = new PagedResult<JobTitleListDto>
        {
            Items = jobTitleDtos,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };

        return new GenericResponse<PagedResult<JobTitleListDto>>
        {
            Success = true,
            Data = pagedResult
        };
    }
}
