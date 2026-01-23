using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Attendance.Queries.GetAttendancesList;

public record GetAttendancesListQuery(
    Guid? EmployeeId = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    Guid? StatusId = null,
    bool? IsLate = null,
    bool? IsOvertime = null,
    string? SortBy = null,
    bool IsDescending = false,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<ErrorOr<GenericResponse<PagedResult<AttendanceListDto>>>>;
