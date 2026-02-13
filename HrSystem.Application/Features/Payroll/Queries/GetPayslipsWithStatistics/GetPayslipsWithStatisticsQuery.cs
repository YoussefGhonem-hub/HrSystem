using ErrorOr;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Payroll.Queries.GetPayslipsWithStatistics;

public record GetPayslipsWithStatisticsQuery(
    int Month,
    int Year,
    int PageNumber = 1,
    int PageSize = 10,
    Guid? EmployeeId = null,
    Guid? DepartmentId = null,
    bool? IsPaid = null,
    string? SearchTerm = null,
    string? SortBy = "EmployeeCode",
    bool SortDescending = false
) : IRequest<ErrorOr<GenericResponse<PayslipsResponseDto>>>;
