using ErrorOr;
using HrSystem.Application.Common.Extensions;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Attendance.Queries.GetAttendancesList;

public record GetAttendancesListQuery(
    Guid? EmployeeId = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    Guid? StatusId = null,
    bool? IsLate = null,
    bool? IsOvertime = null,
    string? SearchTerm = null,
    string? SortBy = null,
    bool IsDescending = false,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<ErrorOr<GenericResponse<PagedResult<AttendanceListDto>>>>;

public class GetAttendancesListQueryHandler : IRequestHandler<GetAttendancesListQuery, ErrorOr<GenericResponse<PagedResult<AttendanceListDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetAttendancesListQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<PagedResult<AttendanceListDto>>>> Handle(
        GetAttendancesListQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Attendances
            .AsNoTracking()
            .ApplyBranchScope()
            .AsQueryable();

        // Apply filters
        query = query.ApplyFilters(
            request.EmployeeId,
            request.FromDate,
            request.ToDate,
            request.StatusId,
            request.IsLate,
            request.IsOvertime,
            request.SearchTerm);

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = query.ApplySorting(request.SortBy, request.IsDescending);

        // Apply pagination
        query = query.ApplyPaging(request.PageNumber, request.PageSize);

        // Project to DTO directly — avoids Include INNER JOIN issues
        var dtos = await query.Select(a => new AttendanceListDto
        {
            Id = a.Id,
            EmployeeId = a.EmployeeId,
            EmployeeName = a.Employee.FirstNameEn + " " + a.Employee.LastNameEn,
            EmployeeCode = a.Employee.EmployeeCode,
            Date = a.Date,
            CheckInTime = a.CheckInTime,
            CheckOutTime = a.CheckOutTime,
            StatusId = a.StatusId,
            StatusNameEn = a.Status.NameEn,
            StatusNameAr = a.Status.NameAr,
            WorkedHours = a.WorkedHours,
            IsLate = a.IsLate,
            IsEarlyLeave = a.IsEarlyLeave,
            IsOvertime = a.IsOvertime
        }).ToListAsync(cancellationToken);

        var pagedResult = new PagedResult<AttendanceListDto>
        {
            Items = dtos,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };

        return new GenericResponse<PagedResult<AttendanceListDto>>
        {
            Success = true,
            Data = pagedResult
        };
    }
}
