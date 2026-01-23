using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Application.Features.Lifecycle.EmployeeAssets.Queries.GetEmployeeAssetsList;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Lifecycle.EmployeeAssets.Queries.GetMyAssetsList;

public class GetMyAssetsListQueryHandler : IRequestHandler<GetMyAssetsListQuery, ErrorOr<GenericResponse<PagedResult<EmployeeAssetListDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetMyAssetsListQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<PagedResult<EmployeeAssetListDto>>>> Handle(
        GetMyAssetsListQuery request,
        CancellationToken cancellationToken)
    {
        Guid? employeeId = CurrentUser.EmployeeId;

        if (!employeeId.HasValue || employeeId.Value == Guid.Empty)
        {
            var userId = CurrentUser.Id;
            if (userId.HasValue)
            {
                employeeId = await _context.Employees
                    .Where(e => e.UserId == userId)
                    .Select(e => e.Id)
                    .FirstOrDefaultAsync(cancellationToken);
            }
        }

        if (!employeeId.HasValue || employeeId.Value == Guid.Empty)
        {
            return Error.Unauthorized("Assets.Unauthorized", "Current user is not linked to an employee");
        }

        var query = _context.EmployeeAssets
            .Include(a => a.Employee)
            .AsQueryable();

        query = query.ApplyFilters(
            employeeId,
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
