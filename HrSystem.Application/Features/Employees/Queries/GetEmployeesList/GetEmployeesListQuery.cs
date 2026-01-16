using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Application.Features.Employees.DTOs;
using HrSystem.Domain.Enums;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Employees.Queries.GetEmployeesList;

public record GetEmployeesListQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    EmployeeStatus? Status = null,
    Guid? DepartmentId = null,
    Guid? BranchId = null,
    Guid? JobTitleId = null,
    Guid? ManagerId = null,
    string? SortBy = null,
    bool SortDescending = false
) : IRequest<ErrorOr<GenericResponse<PagedResult<EmployeeListDto>>>>;
