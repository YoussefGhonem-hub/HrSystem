using ErrorOr;
using HrSystem.Domain.Enums;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.LeaveBalances.Queries.GetLeaveReport;

public record GetLeaveReportQuery(
    int? Year = null,
    Guid? EmployeeId = null,
    Guid? DepartmentId = null,
    Guid? VacationTypeId = null,
    EmployeeRequestStatus? Status = null,
    string? SearchTerm = null,
    int PageNumber = 1,
    int PageSize = 10,
    string? SortBy = "RequestedDate",
    bool SortDescending = true
) : IRequest<ErrorOr<GenericResponse<LeaveReportResponseDto>>>;

