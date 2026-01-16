using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Lifecycle.EmployeeAssets.Queries.GetEmployeeAssetsList;

public record GetEmployeeAssetsListQuery(
    Guid? EmployeeId = null,
    string? AssetType = null,
    bool? IsReturned = null,
    DateTime? AssignedDateFrom = null,
    DateTime? AssignedDateTo = null,
    string? SortBy = null,
    bool IsDescending = false,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<ErrorOr<GenericResponse<PagedResult<EmployeeAssetListDto>>>>;
