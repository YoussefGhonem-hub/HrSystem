using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Lifecycle.EmployeeAssets.Queries.GetEmployeeAssetsList;

public class GetEmployeeAssetsListQueryHandler : IRequestHandler<GetEmployeeAssetsListQuery, ErrorOr<GenericResponse<PagedResult<EmployeeAssetListDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetEmployeeAssetsListQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<PagedResult<EmployeeAssetListDto>>>> Handle(
        GetEmployeeAssetsListQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.EmployeeAssets
            .Include(a => a.Employee)
            .AsQueryable();

        query = query.ApplyFilters(
            request.EmployeeId,
            request.AssetType,
            request.IsReturned,
            request.AssignedDateFrom,
            request.AssignedDateTo);

        var totalCount = await query.CountAsync(cancellationToken);

        query = query.ApplySorting(request.SortBy, request.IsDescending);
        query = query.ApplyPaging(request.PageNumber, request.PageSize);

        var assets = await query.ToListAsync(cancellationToken);

        var dtos = assets.Select(a => new EmployeeAssetListDto
        {
            Id = a.Id,
            EmployeeId = a.EmployeeId,
            EmployeeName = a.Employee?.FullNameEn ?? string.Empty,
            AssetType = a.AssetType,
            AssetName = a.AssetName,
            SerialNumber = a.SerialNumber,
            AssignedDate = a.AssignedDate,
            ReturnDate = a.ReturnDate,
            IsReturned = a.IsReturned,
            Value = a.Value,
            Condition = a.Condition
        }).ToList();

        var pagedResult = new PagedResult<EmployeeAssetListDto>
        {
            Items = dtos,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };

        return new GenericResponse<PagedResult<EmployeeAssetListDto>>
        {
            Success = true,
            Data = pagedResult
        };
    }
}
