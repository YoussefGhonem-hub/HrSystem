using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Branches.Queries.GetBranchesList;

public record GetBranchesListQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    Guid? CountryId = null,
    bool? IsActive = null,
    string? SortBy = null,
    bool SortDescending = false
) : IRequest<ErrorOr<GenericResponse<PagedResult<BranchListDto>>>>;

public class GetBranchesListQueryHandler : IRequestHandler<GetBranchesListQuery, ErrorOr<GenericResponse<PagedResult<BranchListDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetBranchesListQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<PagedResult<BranchListDto>>>> Handle(
        GetBranchesListQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Branches
            .Include(b => b.Employees)
            .AsQueryable();

        query = query.ApplyFilters(request.SearchTerm, request.CountryId, request.IsActive);

        var totalCount = await query.CountAsync(cancellationToken);

        var branches = await query
            .ApplySorting(request.SortBy, request.SortDescending)
            .ApplyPaging(request.PageNumber, request.PageSize)
            .ToListAsync(cancellationToken);

        var branchDtos = branches.Adapt<List<BranchListDto>>();

        var pagedResult = new PagedResult<BranchListDto>
        {
            Items = branchDtos,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };

        return new GenericResponse<PagedResult<BranchListDto>>
        {
            Success = true,
            Data = pagedResult
        };
    }
}
