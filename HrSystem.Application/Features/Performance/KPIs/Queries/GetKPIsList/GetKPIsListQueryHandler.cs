using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.KPIs.Queries.GetKPIsList;

public class GetKPIsListQueryHandler : IRequestHandler<GetKPIsListQuery, ErrorOr<GenericResponse<PagedResult<KPIListDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetKPIsListQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<PagedResult<KPIListDto>>>> Handle(
        GetKPIsListQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.KPIs
            .Include(k => k.JobTitle)
            .Include(k => k.Department)
            .AsQueryable();

        // Apply filters
        query = query.ApplyFilters(
            request.SearchTerm,
            request.Category,
            request.JobTitleId,
            request.DepartmentId,
            request.WeightMin,
            request.WeightMax);

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting and pagination
        var kpis = await query
            .ApplySorting(request.SortBy, request.SortDescending)
            .ApplyPaging(request.PageNumber, request.PageSize)
            .ToListAsync(cancellationToken);

        // Map to DTOs
        var kpiDtos = kpis.Select(k => new KPIListDto
        {
            Id = k.Id,
            NameAr = k.NameAr,
            NameEn = k.NameEn,
            Category = k.Category,
            Weight = k.Weight,
            JobTitleEn = k.JobTitle?.TitleEn,
            JobTitleAr = k.JobTitle?.TitleAr,
            DepartmentNameEn = k.Department?.NameEn,
            DepartmentNameAr = k.Department?.NameAr,
            CreatedDate = k.CreatedDate.DateTime
        }).ToList();

        var pagedResult = new PagedResult<KPIListDto>
        {
            Items = kpiDtos,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };

        return new GenericResponse<PagedResult<KPIListDto>>
        {
            Success = true,
            Data = pagedResult
        };
    }
}
