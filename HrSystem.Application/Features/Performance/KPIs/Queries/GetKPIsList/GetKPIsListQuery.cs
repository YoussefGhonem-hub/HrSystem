using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Performance.KPIs.Queries.GetKPIsList;

public record GetKPIsListQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    string? Category = null,
    Guid? JobTitleId = null,
    Guid? DepartmentId = null,
    int? WeightMin = null,
    int? WeightMax = null,
    string? SortBy = null,
    bool SortDescending = false
) : IRequest<ErrorOr<GenericResponse<PagedResult<KPIListDto>>>>;
