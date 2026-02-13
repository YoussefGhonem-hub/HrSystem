using ErrorOr;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Payroll.Queries.GetPayrollOverview;

public record GetPayrollOverviewQuery(
    int Month,
    int Year,
    int PageNumber = 1,
    int PageSize = 10,
    Guid? DepartmentId = null,
    Guid? BranchId = null,
    string? Status = null,
    string? SearchTerm = null,
    string? SortBy = "DepartmentName",
    bool SortDescending = false
) : IRequest<ErrorOr<GenericResponse<PayrollOverviewResponseDto>>>;
